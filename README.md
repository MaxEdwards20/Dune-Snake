# Dune-Snake

<!-- TODO ![Gameplay Image](./gameplay.png) -->

A game of Snake built using C# in the MonoGame framework, themed around everyone's favorite Sci-Fi franchise: [Dune](https://www.sfgate.com/sf-culture/article/dune-part-two-review-18678628.php).

<!-- ## Project Description -->

<!-- TODO -->
<!-- Screenshots: -->

## Notes

- For tomorrow: Solve the keyboard input problem with the entities and components. I am thinking I will keep the MenuKeyboardInput and add the controls directly to the system keyboard input.

For office hours:

- How would you suggest we save the user controls? I am trying to think of this in an entity component system way. I am thinking of having a component that holds the controls and then a system that reads the controls and updates the entity. I am not sure if this is the best way to do it.
- We already have the keyboard input system. Would that be the best place to put the controls? How do give the gameplay access to the controls component?


## Debugging

### Connection Refused
- Try switching ports. A few options are 4000, 4010, 3000, 3010
- Use netcat on Mac or Linux to see whether the server is available.
    `nc -vz 192.168.4.20 3000`
- Turn off your firewall. On macOS that is found in System Preferences > Security & Privacy > Firewall