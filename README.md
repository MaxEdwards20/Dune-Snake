# Dune-Snake

![Gameplay Image](./gameplay.png)

A game of Snake built using C# in the MonoGame framework, themed around everyone's favorite Sci-Fi franchise: [Dune](https://www.sfgate.com/sf-culture/article/dune-part-two-review-18678628.php).

<!-- ## Project Description -->

## How to Setup
1. Install [MonoGame](https://docs.monogame.net/articles/getting_started/index.html) on your local machine.
2. Open [this](./src/DuneSnake.sln) file in Visual Studio, Jet Brains Ryder, or another compatible IDE.
3. Start the server
4. Start the client
5. Start playing!

## Debugging

### Connection Refused

- Try switching ports. A few options are 4000, 4010, 3000, 3010
- Use netcat on Mac or Linux to see whether the server is available.
  `nc -vz 192.168.4.20 3000`
- Turn off your firewall. On macOS that is found in System Preferences > Security & Privacy > Firewall

### Client Prediction and Server Reconcilition

- When input occurs the client does that and send it to the server
- The server receives the client input and upates the game state
- Server Reconcilition happens on the client side for the items that have not been acknowledged by the server
