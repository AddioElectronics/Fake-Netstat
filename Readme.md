# Fake Netstat

Fake Netstat is scam baiting software, for tricking tech support scammers.
You can make the list very short, and very slow, or the opposite being extremely fast and infinitely long.

Inspired by Kitboga, who said in one of his recent videos that he is using a fake netstat, and so I thought it would be a fun little excercise to do this afternoon.

## Installation

Note: This should only be used on a virtual machine.

Backup and replace the NETSTAT.exe in these locations.

- C:\Windows\SysWOW64

- C:\Windows\System32

- C:\Windows\WinSxS\amd64_microsoft-windows-tcpip-utility_31bf3856ad364e35_10.0.18362.1_none_052463a5cc169193

- C:\Windows\WinSxS\wow64_microsoft-windows-tcpip-utility_31bf3856ad364e35_10.0.18362.1_none_0f790df80077538e

You will have to figure out on your own how to overwrite these files, as there is more to it than just getting a security prompt.

If you want to test it out, just rename the fake netstat executable and run it from the command line.

## Usage

Run netstat like normal using cmd.exe

#### Command Arguments

I've added 2 custom commands,

- /config - Will open the configuration folder for easy access to the config file.
- /profile # - Will set the profile in the config.

## Configuring

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
- Custom Connections - An array of pre-built connections, allows you to customize the protocol, local IP, foreign IP, and the state.
- Custom Connection Formats - An array of semi-built connections, just like Custom Connections but allows you to randomize things like digits. 

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
-{LOCAL}   - Will be replaced with the local IP.
-{D255}    - Will be replaced with a digit from 0 to 255
-{PORT}    - Will be replaced with a random port.

```

## Contributing
I encourage you to pull and make changes, I will not be working on this more than this one afternoon, but I will still manage and merge changes I approve.

One thing that needs to be done is taking the real netstat arguments into account, they are being parsed, but nothing is happening with the data.

Also the help text is displayed, but only when you type help, /? or ?, the regular netstat will display it when ever an invalid argument is passed, so that is also something that should be changed.


## License
No license, you are free to do what ever you want.