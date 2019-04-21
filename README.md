# Octave
Goverlay plugin for serial communication

Octave is a goverlay plugin intended to use with the following hardware/softare : http://www.goverlay.com/
It provides serial communication support for sensor reading.

It was formerly made for a DIY project (displaying on LCDSysInfo 3.5" the values of several sensor plugged
in an arduino, the communication between the arduino and goverlay is done with serial (COM) port
on the target computer.)
Application was to display water flow and temperature on a dual loop watercooled computer.

--------------------------------------------------------------------------------------------------------------
Serial (COM) traffic should respect the following :

- No unsollicited traffic should be done on serial port (debug traces or other ...)

Following query/request are available :<br>
Computer request : GET<br>
Device reply : GET|sensor1:value1|sensor2:value2|.....|sensorN:valueN<br>
Where sensorX is the name of the sensor and valueX is the current value of this sensor.<br>
There is no limitation (except buffer size) to the count of sensor<br> 
--------------------------------------------------------------------------------------------------------------

Installation :
1) Stop Goverlay
2) Copy bin\Debug\Octave.dll into Plugins directory of Goverlay
3) Make sure your serial device is running and restart Goverlay
4) Activate plugin and edit plugins Options (COM port number, speed ...)
5) Edit your profile : the new sensors should be visible

Designing your own serial device respecting this syntax should allow you to display your own sensors
on LCDSysInfo using goverlay with Octave as a plugin

For those who want to know, Octave is named after my 8yo cat I got from a shelter. 

Olivier Galand, France (2019)
