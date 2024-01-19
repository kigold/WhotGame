# Whotapp

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 16.0.1.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.

TASKS
# Implement Get Games
- Implement Create Game and Join Game
  - Add a Play Game button to the homepage, when a user clicks it, it will call an endpoint that would check if there is any pending game that is not yet filled to capacity (say 10),
    if none, it will create a new Game and add the user to it, and then set a timer for Game to start (2 mins), if after 2 mins no other player joins the game, the game will
    be aborted and the user will be notified. once anyone joins the game, the message is sent to all user connected to the game. Game will start when the timer completes the set time,
    or when the game is filled to capacity, whichever comes first
  - implement join logic when user clicks button
  - redirect user to the game page when they click, show user the status of the game, the countdown clock, and the other players
- Create Game Screen
- Create Card
- Create Game Viewer, where audience can view a game as it is happening without interfering
  - add view button to the list of games, that when it is clicked it will lunch a page where user can view the game events (kinda like readonly game play)
- 

Done
* Implement Get Games
 - load games on the home page
 - Get list view component to list game
 - Listen to Hub for newly created Games, and add it to the list
* Implement Create Game and Join Game
 * Add Button to Play Game on the home page
 - Create or Get Game existing pending game to join
 - After Clicking, redirect to the Game Page, waiting for others to join
 - Receive notification from hub of players that have joined game
 - Start game after count down, if no other player joins then end game and return to homepage
 - 
TODO
* FE, Design Game Screen
 - Consume Get GameStats, to display game stats on game page
 - Create Card Component
 - Create Carousel to Display player Cards
 - Implement Click and Select to play Card
 - Implement Arrow to select and Enter to Play Card
 - Implement timer when game starts a time is set for the next player to play, when they play, then reset the timer for the next person to play 1 mins or 2 mins
 - - Prompt the user when the time is up and give 30 secs grace, this should be configurable, if the user still does not play then end game or kick the user out if there are more than 1 other users
 - Implement AI feature that can play the game, this can be used for testing and also for a user to play with the AI
 - 
 - 
