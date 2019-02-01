# ElevatorKata
These are some features. They can be implemented in any order you prefer.

For some Domain history look at https://en.wikipedia.org/wiki/Storey for a better understanding

* (Done) Elevator responds to calls containing a source floor and returns a direction
* (Done) Elevator delivers passengers to requested floors
* (Done) Elevator should have an event that emits each floor as it passes the floor
* (Done) Elevator doesn't respond immediately. consider options to simulate time
* (Done) Elevator calls are queued not necessarily FIFO
* (Done) Validate passenger floor requests
* (Done as Observer or Event)Implement current floor monitor
* (Done) Implement direction arrows
* (Done) You may implement doors (opening and closing)
* (Done) You may implement DING!
* The can be an internal elevator display panel that can command the elevator, can open the door if the lift is on the same floor and stopped, can close the door if the door is open and stopped and with floor buttons to command the lift to the floor, showing active until the floor is reached
* (Done) There can be an external elevator display on each floor that can be responsible for calling an elevator, showing an active colour in the up or down direction depending on which button is selected
* There can be an external elevator with up and down arrows that displays the direction, if any, the lift is going in and turns off if the lift is on the same floor closed with no other floor requests
* There can be a three story building containing two lifts, each floor containing an external up down arrow panel above the lift, to the right of the lift an external up down command panel and inside the lift is an internal display panel for users to command the lift