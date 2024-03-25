
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using System;
using System.Collections.Generic;
using Shared.Components;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Client.Menu;

namespace Client.Systems
{

    public class KeyboardInput : Shared.Systems.System
    {
        
        private HashSet<Keys> m_keysPressed = new HashSet<Keys>();

        private KeyboardState m_statePrevious = Keyboard.GetState();

        public KeyboardInput(List<Tuple<Shared.Components.Input.Type, Keys>> mapping) : base(typeof(Shared.Components.Input))
        {
            LoadControls();
        }

        public override void update(TimeSpan elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            m_keysPressed.Clear();

            foreach (var key in keyboardState.GetPressedKeys())
            {
                m_keysPressed.Add(key);
            }

            foreach (var entity in m_entities)
            {
                var controls = entity.Value.get<Controls>();
                var inputs = new List<Input.Type>();

                checkAndPerformAction(controls.SnakeUp, Input.Type.SnakeUp, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(controls.SnakeLeft, Input.Type.RotateLeft, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(controls.SnakeRight, Input.Type.RotateRight, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(controls.SnakeDown, Input.Type.SnakeDown, entity.Value, elapsedTime, inputs);
                checkAndPerformAction(controls.SnakeBoost, Input.Type.Boost, entity.Value, elapsedTime, inputs);

                if (inputs.Count > 0)
                {
                    // Assuming you have a messaging system to handle input
                    MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(entity.Key, inputs, elapsedTime));
                }
            }
        }
        
        private void checkAndPerformAction(Control control, Input.Type inputType, Entity entity, TimeSpan elapsedTime, List<Input.Type> inputs)
        {
            if (m_keysPressed.Contains(control.key))
            {
                inputs.Add(inputType);

                // Perform action based on inputType
                switch (inputType)
                {
                    case Input.Type.SnakeUp:
                        Utility.thrust(entity, elapsedTime);
                        break;
                    case Input.Type.Boost:
                        Utility.boost(entity, elapsedTime);
                        break; 
                    case Input.Type.RotateLeft:
                        Utility.rotateLeft(entity, elapsedTime);
                        break;
                    case Input.Type.SnakeDown:
                        Utility.thrust(entity, elapsedTime);
                        break;
                    case Input.Type.RotateRight:
                        Utility.rotateRight(entity, elapsedTime);
                        break;
                }
            }
        }
        
        public void updateControlKey(Control v, Keys key)
        {
            v.switchKey(key);
            SaveControls();
        }

        public override bool add(Entity entity)
        {
            if (!base.add(entity))
            {
                return false;
            }
            return true;
        }

        public override void remove(uint id)
        {
            base.remove(id);
        }

        public void keyPressed(Keys key)
        {
            m_keysPressed.Add(key);
        }

        public void keyReleased(Keys key)
        {
            m_keysPressed.Remove(key);
        }
        

        

    }
}
