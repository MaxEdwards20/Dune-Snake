﻿
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using Shared.Entities;
using Shared.Systems;
using Input = Shared.Components.Input;

namespace Client.Systems
{
    public class Network : Shared.Systems.System
    {
        public delegate void Handler(TimeSpan elapsedTime, Shared.Messages.Message message);
        public delegate void RemoveEntityHandler(RemoveEntity message);
        public delegate void NewEntityHandler(NewEntity message);
        public delegate void CollisionHandler(Collision message);
        public delegate void NewAnchorPointHandler(NewAnchorPoint message);
        
        private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();
        private RemoveEntityHandler m_removeEntityHandler;
        private NewEntityHandler m_newEntityHandler;
        private CollisionHandler m_collisionHandler;
        private NewAnchorPointHandler m_newAnchorPointHandler;
        private uint m_lastMessageId = 0;
        private HashSet<uint> m_updatedEntities = new HashSet<uint>();
        private PlayerData m_playerData;
        

        /// <summary>
        /// Primary activity in the constructor is to setup the command map
        // that maps from message types to their handlers.
        /// </summary>
        public Network(String playerName, PlayerData playerData) :
            base(typeof(Shared.Components.Position))
        {
            m_playerData = playerData;
            registerHandler(Shared.Messages.Type.ConnectAck, (TimeSpan elapsedTime, Message message) =>
            {
                handleConnectAck(elapsedTime, (ConnectAck)message, playerName);
            });

            registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
            {
                m_newEntityHandler((NewEntity)message);
            });

            registerHandler(Shared.Messages.Type.UpdateEntity, (TimeSpan elapsedTime, Message message) =>
            {
                handleUpdateEntity(elapsedTime, (UpdateEntity)message, m_playerData);
            });

            registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
            {
                m_removeEntityHandler((RemoveEntity)message);
            });
            
            registerHandler(Shared.Messages.Type.Collision, (TimeSpan elapsedTime, Message message) =>
            {
                m_collisionHandler((Collision)message);
            });
            
            registerHandler(Shared.Messages.Type.NewAnchorPoint, (TimeSpan elapsedTime, Message message) =>
            {
                m_newAnchorPointHandler((NewAnchorPoint)message);
            });
            
        }

        // Have to implement this because it is abstract in the base class
        public override void update(TimeSpan elapsedTime) { }

        /// <summary>
        /// Have our own version of render, because we need a list of messages to work with, and
        /// messages aren't entities.
        /// </summary>
        public void update(TimeSpan elapsedTime, Queue<Message> messages, PlayerData playerData)
        {
            m_updatedEntities.Clear();

            if (messages != null)
            {
                while (messages.Count > 0)
                {
                    var message = messages.Dequeue();
                    if (m_commandMap.ContainsKey(message.type))
                    {
                        m_commandMap[message.type](elapsedTime, message);
                    }

                    if (message.messageId.HasValue)
                    {
                        m_lastMessageId = message.messageId.Value;
                    }
                }
            }
            // After processing all the messages, perform server reconciliation by
            // resimulating the inputs from any sent messages not yet acknowledged by the server.
            
            // This is where all of the other worms are updated
            var sent = MessageQueueClient.instance.getSendMessageHistory(m_lastMessageId);
            while (sent.Count > 0)
            {
                var message = (Shared.Messages.Input)sent.Dequeue();
                if (message.type == Shared.Messages.Type.Input && m_entities.ContainsKey(message.entityId))
                {
                    var entity = m_entities[message.entityId];
                    Debug.Assert(entity.contains<Head>());
                    var worm = WormMovement.getWormFromHead(entity, m_entities);
                    if (m_updatedEntities.Contains(entity.id))
                    {
                        foreach (var input in message.inputs)
                        {
                            switch (input)
                            {
                                case Shared.Components.Input.Type.PointLeft:
                                    Shared.Systems.WormMovement.left(worm, message.elapsedTime);
                                    break;
                                case Shared.Components.Input.Type.PointRight:
                                    Shared.Systems.WormMovement.right(worm, message.elapsedTime);
                                    break;
                                case Shared.Components.Input.Type.PointUp:
                                    Shared.Systems.WormMovement.up(worm);
                                    break;
                                case Shared.Components.Input.Type.PointDown:
                                    Shared.Systems.WormMovement.down(worm);
                                    break;
                                case Input.Type.PointUpLeft:
                                    Shared.Systems.WormMovement.upLeft(worm);
                                    break;
                                case Input.Type.PointUpRight:
                                    Shared.Systems.WormMovement.upRight(worm);
                                    break;  
                                case Input.Type.PointDownLeft:
                                    Shared.Systems.WormMovement.downLeft(worm);
                                    break;
                                case Input.Type.PointDownRight:
                                    Shared.Systems.WormMovement.downRight(worm);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void registerHandler(Shared.Messages.Type type, Handler handler)
        {
            m_commandMap[type] = handler;
        }

        public void registerNewEntityHandler(NewEntityHandler handler)
        {
            m_newEntityHandler = handler;
        }

        public void registerRemoveEntityHandler(RemoveEntityHandler handler)
        {
            m_removeEntityHandler = handler;
        }
        
        public void registerCollisionHandler(CollisionHandler handler)
        {
            m_collisionHandler = handler;
        }
        
        public void registerNewAnchorPointHandler(NewAnchorPointHandler handler)
        {
            m_newAnchorPointHandler = handler;
        }

        /// <summary>
        /// Handler for the ConnectAck message.  This records the clientId
        /// assigned to it by the server, it also sends a request to the server
        /// to join the game.
        /// </summary>
        private void handleConnectAck(TimeSpan elapsedTime, ConnectAck message, string name) 
        {
            MessageQueueClient.instance.sendMessage(new Join(name));
        }

        /// <summary>
        /// Handler for the UpdateEntity message.  It checks to see if the client
        /// actually has the entity, and if it does, updates the components
        /// that are in common between the message and the entity.
        /// </summary>
        private void handleUpdateEntity(TimeSpan elapsedTime, UpdateEntity message, PlayerData playerData)
        {
            if (m_entities.ContainsKey(message.id))
            {
                var entity = m_entities[message.id];
                if (entity.contains<Components.Goal>() && message.hasPosition)
                {
                    var position = entity.get<Position>();
                    var goal = entity.get<Components.Goal>();

                    goal.updateWindow = message.updateWindow;
                    goal.updatedTime = TimeSpan.Zero;
                    goal.goalPosition = new Vector2(message.position.X, message.position.Y);
                    goal.goalOrientation = message.orientation;

                    goal.startPosition = position.position;
                    goal.startOrientation = position.orientation;
                }
                else if (entity.contains<Position>() && message.hasPosition)
                {
                    entity.get<Position>().position = message.position;
                    entity.get<Position>().orientation = message.orientation;
 
                    m_updatedEntities.Add(entity.id);
                }
                if (entity.contains<SpicePower>() && message.hasSpicePower)
                {
                    entity.get<SpicePower>().setPower(message.spicePower);
                }
                
                if (entity.contains<ParentId>() && message.hasParent)
                {
                    var q = entity.get<AnchorQueue>();
                    entity.remove<ParentId>();
                    entity.add(new ParentId(message.parentId));
                }
                if (entity.contains<ChildId>() && message.hasChild)
                {
                    entity.remove<ChildId>();
                    entity.add(new ChildId(message.childId));
                }
                if (entity.contains<Stats>() && message.hasStats)
                {
                    Stats stats = entity.get<Stats>();
                    stats.Kills = message.Kills;
                    stats.Score = message.Score;
                    if (entity.contains<Name>() && entity.get<Name>().name == m_playerData.playerName)
                    {
                        playerData.score = (int) message.Score;
                        playerData.setKills((int)message.Kills);
                        // Update the highest position based on all of the entities
                        List<Entity> heads = new();
                        foreach (var ent in m_entities.Values)
                        {
                            if (ent.contains<Head>())
                            {
                                heads.Add(ent);
                            }
                        }
                        heads.Sort((a, b) => a.get<Stats>().Score.CompareTo(b.get<Stats>().Score));
                        heads.Reverse(); // Largest to smallest
                        playerData.addPosition(heads.IndexOf(entity) + 1);
                    }
                }
            }
        }
    }
}

