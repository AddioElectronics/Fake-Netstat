# Fake Netstat

Fake Netstat is a replacement executable, for tricking tech support scammers.
You can make the list very short, and very slow, or the opposite being extremely fast and infinitely long.

Inspired by Kitboga, who said in one of his recent videos that he is using a fake netstat, and so I thought it would be a fun little excercise to do this afternoon.

## Installation

Note: You should only be replacing the executables on a Virtual Machine. Try it out in visual studio first.

Backup and replace the NETSTAT.exe in these locations, with the NETSTAT.exe from bin\Release.
```

- C:\Windows\SysWOW64

- C:\Windows\System32

- C:\Windows\WinSxS\amd64_microsoft-windows-tcpip-utility_31bf3856ad364e35_10.0.18362.1_none_052463a5cc169193

- C:\Windows\WinSxS\wow64_microsoft-windows-tcpip-utility_31bf3856ad364e35_10.0.18362.1_none_0f790df80077538e
```

You will have to figure out on your own how to rename/overwrite these files, as there is more to it than just getting a security prompt. Windows has extra security for these applications, and even after the elevated prompt, it will still stop you.

If you want to test it out, just rename the fake netstat executable and run it from the command line, or just run from inside visual studio.


## Usage

Run netstat like normal using cmd.exe

#### Command Arguments

I've added 2 custom commands,

- /config - Will open the configuration folder for easy access to the config file.
- /profile # - Will set the profile in the config.

#### Configuring

After the first time running, it will create a config file at %Temp%\Addio\Antiscam\Netstat\config.json

You can change how the program runs by editing it in a text editor.

- Profile - Allows you to quickly change multiple settings with a single value.
```
0. No Profile
1. Snails pace, with 100 connections
2. Slow and short
3. Slow and Long
4. Never Ending
5. Fast Short
6. Fast Long
7. Fast Never Ending
8. Very Fast Long
9. Very Fast Never Ending
10. Light Speed
```

- Connection Count  - How many connections will be displayed in the list.
- Loop Back Connection Count - How many loop back connections will be displayed in the list.
- Interval Min - The minimum time in between displaying each command, in milliseconds.
- Interval Max - The maximum time in between displaying each command, in milliseconds.
- Port Min - The lowest value a fake port will be.
- Port Max - The highest value a fake port will be.
- Seed - The seed for the random number generator.
- Random Seed - If false it will disregard seed, and generate it using a time based method.
- Use Real Connections - Allows you to mix real connections that netstat would normally display in with the fake connections.
- Real Chance (0 - 1) - The chance a real connection will be displayed. (0 - 1)
- Custom Chance (0 - 1) - The chance a custom connection will be displayed. (0 - 1)
- Allow Repeated Connections- Can custom, and real commands be displayed more than once? If your list is short this is not recommended.
- Randomize Custom Order - If true the custom commands will be displayed in a random order.
- Custom Connections - An array of pre-built connections, allows you to customize the protocol, local IP, foreign IP, and the state. These can be toggled to repeat or not.
- Custom Connection Formats - An array of semi-built connections, just like Custom Connections but allows you to randomize things like digits. These will always have a chance of repeating.

Custom Connection Formats Example:
```
{
   "protocol": 0,
   "local_address": "{LOCAL}",
   "foreign_address": "ec{D255}-{D255}-{D255}-{D255}-{D255}:{PORT}",
   "state": 4 
},

-protocol  = enum Proto { TCP = 0, UDP = 1, TCPv6 = 2, UDPv6 = 3, RANDOM = 4 }
-State     = public enum State { ESTABLISHED = 0, TIME_WAIT = 1, LAST_ACK = 2, CLOSE_WAIT = 3, RANDOM = 4 }

 Format Token
-{LOCAL}   - Will be replaced with the local IP.
-{D255}    - Will be replaced with a digit from 0 to 255
-{PORT}    - Will be replaced with a random port.

```

- Fake Processes - An array of strings for when -b is passed as an argument. If there is none in the list, names of real processes will be used.
- Use Real Process Names - When -b is passed as an argument, should real process names be displayed? If no fake processes were created, this will always be true.

## Contributing
I encourage anyone to pull and make changes, I will not be working on this more than this one afternoon(Other than packing DLLs into build), but I will still manage and merge changes I approve.

One thing that could be done is dealing with more netstat arguments. Here is a list of arguments and their status. Not very important, these technicians aren't very good with PC's, and there is almost 0 chances they will ever pass an argument.
```
-a : Not Implemented, would take a lot of work.
-b : Fully faked!
-e : Partial implementation, only displays constant string. Would be easy to make dynamic.
-f : Not Implemented
-n : Implemented
-o : Partial implementation, only displays constant string.
-p : Implemented
-q : Not Implemented
-r : Partial implementation, only displays constant string.
-s : Not Implemented
-t : Partial implementation, only displays constant string.
-x : Not Implemented
-y : Partial implementation, only displays constant string.

```

Another thing that could be done is adding more tokens/wildcards for the custom connections in the config file. The way its implemented right now is bad, to add more tokens easily the section of code will have to be refactored.

Other than that, there isn't really much to be done, other than adding silly features that could create a laugh or two.

## License
No license, you are free to do what ever you want. But if you want to give me credit, you are welcome to do so.

## Credit

- Author   : Addio from Addio Electronics
- Website  : www.Addio.io
