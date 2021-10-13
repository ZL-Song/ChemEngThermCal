# ChemEngThermCal

This is something I did a LOoOoOoOng~~~ time ago: a calculator for chemical engineering thermodynamics.  
That is: macroscopic equation of states, vapor-liquid equilibrium (activity/fugacity), etc.

There is a binary executable in the ChemEngThermCal/bin folder, which in theory should run on any windows OS with .net framework version > 4.0. 

All projects were intended to be OOP.
There are ~20,000 lines of C# code.
The GUI is based on WPF with customized controls, which I found very fancy.

- ChemEngThermCal:         The main program, mostly the graphic interface and animations (advantages of WPF), also calls the algorithm;
- ChemEngThermCal.Model:   Implements the algorithms, note that all comments are written in Chinese;
- ChemEngThermCal.Control: Here I defined some of the styles for the GUI controls with WPF, including Buttons, Checkboxes, Dropboxes, etc.

By then, my coding was naive and my english was crappy so please excuse.
I am solely responsible for the codes in here and I do not guarantee the correctness of the results calculated from this program. 
