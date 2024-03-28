# Dune-Snake

<!-- TODO ![Gameplay Image](./gameplay.png) -->

A game of Snake built using C# in the MonoGame framework, themed around everyone's favorite Sci-Fi franchise: [Dune](https://www.sfgate.com/sf-culture/article/dune-part-two-review-18678628.php).

<!-- ## Project Description -->

<!-- TODO -->
<!-- Screenshots: -->

## In Progress

- [ ] MAX: Decide how to build the snake. Tons of entities? Or one entity with a list of positions?
- [ ] Satchel: Pick head, body, and tail texture for the sandworm

## Items to Develop

- [ ] Pick head, body, and tail texture for the sandworm - Satchel 
- [ ] Animated Sprite for the spice - Satchel 
- [ ] Collision detection. Know whether we hit spice or another sandworm
- [ ] Map generation
- [ ] Let player name themselves
- [ ] Mouse input support - Satchel 
- [ ] Food generation that refreshes as we play the game
- [ ] On collision, sandworm breaks apart and is available as food for other snakes
- [ ] Record players score, kills and highest position. Probably can be added to the `GameScores` object.
- [ ] Game over screen with score, kills, and highest position achieved
- [ ] Particle system for the death of a sandworm
- [ ] Sound effects on death of worm and when food is eaten - Satchel 

## Debugging

### Connection Refused

- Try switching ports. A few options are 4000, 4010, 3000, 3010
- Use netcat on Mac or Linux to see whether the server is available.
  `nc -vz 192.168.4.20 3000`
- Turn off your firewall. On macOS that is found in System Preferences > Security & Privacy > Firewall
