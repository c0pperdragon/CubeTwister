# CubeTwister
Rubik's Cube solving robot using 2 Lego Mindstorm EV3 kits and additional hardware

This is my third version of a Rubik's Cube solver using Lego parts for the 
mechanics. To achieve maximum speed, two EV3 bricks are used that drive 10 motors.

Two motors each drive one axis of the machine. This gives greater speed and accuracy also, because
two motors together can take up the slack of the gear trains. Because there is only a total of 8 motor ports 
on the two EV3 bricks, two of the motors are not controlled individually but share a port with their partner
on the same axis. 

The whole configuration is designed to lock a rubik's cube in place by holding onto 5 of the center cubies. 
Each of the 5 connected sides can be twisted independently by directly turning the center. This does not allow
twisting of the the 6th side (the top one), so the solving algorithm must work around this limitation.
Nevertheless every situation of the cube can be solved by twisting only 5 sides with the slight penalty
of using a more moves.

The scanning of the cube is done with 8 simultaneously running color sensors. These color sensor are
custom made from RGB-LEDs and light sensor diodes and controlled by an Arduino Micro connected 
to the EV3. 

The main control program and the solver is implemented in EV3Basic (https://github.com/c0pperdragon/EV3Basic)
and runs on one of the EV3s. To achieve maximum speed even if the program runs in an interpreted VM, the
solving algorithm (a basic two-phase algorithm) uses precomputed algorithm tables for each of the two stages,
which are stored on an SDCARD (using a total of about 6 GByte). Computation of these tables was done on the PC by a 
.NET program and took several days to finish.

Currently (2015) this machine is the word's fastest rubik's cube solver, but it can achieve this speed only by 
having a modified cube (connector plates in the center pieces) and by the operator inserting the cube in a specific 
way (orange center piece on top, white center piece to the back side).
So this could be considered cheating and the machine may be not legible for a world record 
(which currently is about 3.2 seconds, compared to the 1.9 seconds for one of my solve runs).
Neverthless it was fun building the machine, anyway.

Enjoy my video on youtube: https://www.youtube.com/watch?v=s2tCAf6yYoo
	