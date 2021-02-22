using System;
using System.Text.Json.Serialization;
using static Addio.Antiscam.Fake.Netstat.Program;

namespace Addio.Antiscam.Fake.Netstat
{

    [Serializable]
    public class Config
    {

        [JsonInclude]
        /// <inheritdoc cref="Profile"/>
        public Profile profile = Profile.None;

        /// <summary>
        /// How many connections will be displayed?
        /// </summary>
        [JsonInclude]
        public int connectionCount = 1000;

        /// <summary>
        /// How many loopback address to show at the beginning?
        /// These are IPs that start with 127.0.0.1
        /// </summary>
        [JsonInclude]
        public int loopBackConectionCount = 10;

        /// <summary>
        /// The minimum time it will take before a connection is "found."
        /// </summary>
        [JsonInclude]
        public int interval_min = 10;

        /// <summary>
        /// The maximum time it will take before a connection is "found."
        /// </summary>
        [JsonInclude]
        public int interval_max = 400;

        /// <summary>
        /// The smallest number a random port will be.
        /// </summary>
        [JsonInclude]
        public int port_min = 5000;

        /// <summary>
        /// The largest number a port will be.
        /// </summary>
        [JsonInclude]
        public int port_max = 70000;

        /// <summary>
        /// The seed for the random number generator.
        /// </summary>
        [JsonInclude]
        public int seed = 69;

        /// <summary>
        /// Will <see cref="seed"/> be used, or will it generate one?
        /// </summary>
        [JsonInclude]
        public bool random_seed = true;

        /// <summary>
        /// Do you want to mix in real connections with the fake ones?
        /// </summary>
        [JsonInclude]
        public bool useRealConnections = true;

        /// <summary>
        /// How often will a real connection be displayed versus the others?
        /// </summary>
        [JsonInclude]
        public float real_chance = 0.3f;

        /// <summary>
        /// How often will a completely random connection be displayed versus a custom one?
        /// </summary>
        [JsonInclude]
        public float custom_chance = 0.5f;

        /// <summary>
        /// Can it show custom connections more than once?
        /// This is only true for <see cref="custom_connections"/> as <see cref="custom_connection_formats"/> adds random numbers.
        /// </summary>
        [JsonInclude]
        public bool allowRepeatedConnections;

        /// <summary>
        /// Custom connections to display in the list.
        /// Will only be displayed once if <see cref="allowRepeatedConnections"/> is false.
        /// Can only randomize the protocol, state, and port.
        /// </summary>
        [JsonInclude]
        public Connection[] custom_connections = new Connection[] { new Connection { exeName = "grandson-web-tracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "jagex:https", state = Program.State.ESTABLISHED, pid = -1 },
                                                                    new Connection { exeName = "NotaBackdoor.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "anonymous:onion", state = Program.State.ESTABLISHED, pid = 69420  },
                                                                    new Connection { exeName = "grandson-web-tracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "porn-hub:https", state = Program.State.ESTABLISHED , pid = -1 },
                                                                    new Connection { exeName = "grandson-web-tracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "only-fans:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "travel-india:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "grandson-web-tracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "neopets:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "ashley-maddison:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "bbw-india-girls:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "mail-men:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "omegle:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "pop-up.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "penile-enhancement-pills:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "marry-india-woman:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "grandon-web-tracker:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "grandson-web-tracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "blacked:https", state = Program.State.ESTABLISHED , pid = -1 },
                                                                    new Connection { exeName = "NotaBackdoor.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "legion-of-doom:onion", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "world-wide-web-world-wide-tech-support:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "4-chan:https", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "legion-backdoor.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "legion-of-doom:onion", state = Program.State.ESTABLISHED , pid = -1 },
                                                                    new Connection { exeName = "not-a-red-hat.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "black-hat:onion", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "chaos-backdoor.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "chaos-hackers:onion", state = Program.State.ESTABLISHED, pid = -1  },
                                                                    new Connection { exeName = "ScammerLocationTracker.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "fbi-antiscam-unit:onion", state = Program.State.ESTABLISHED , pid = 666 },
                                                                    new Connection { exeName = "edge.exe", protocol = Proto.TCP, local_address = Strings.CustomFormatID.localIP, foreign_address = "india-mail-brides:https", state = Program.State.ESTABLISHED, pid = -1  }};

        /// <summary>
        /// Custom formats for connections. Currently only inserts numbers compared to <see cref="custom_connections"/>.
        /// Will be displayed an infinite amount of times.
        /// </summary>
        [JsonInclude]
        public Connection[] custom_connection_formats = new Connection[] { new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address = "sea{D255}s{D255}-in-f10:https", state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="ec{D255}-{D255}-{D255}-{D255}-{D255}:" + Strings.CustomFormatID.port, state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="a{D255}-{D255}-{D255}-{D255}-{D255}:"+ Strings.CustomFormatID.port, state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="nyidt:{PORT}", state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="kit{D69}bo{D420}ga:{PORT}", state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="sca{D69}mme{D420}rp{D69}ayb{D69}ack:{PORT}", state = Program.State.RANDOM},
                                                                           new Connection { exeName = Strings.CustomFormatID.executable , local_address = Strings.CustomFormatID.localIP, foreign_address ="gym{D99}bro{D99}wni{D99}ng:{PORT}", state = Program.State.RANDOM}};

        /// <summary>
        /// If the user passes -e, it will display process names with the connections.
        /// A random process name will be selected from the list of real running processes, combined with this list.
        /// </summary>
        [JsonInclude]
        public string[] fakeProcesses = new string[] { "NotaBackdoor.exe", "scvhost.exe" };


        /// <summary>
        /// If you don't want to populate the list of fake processes for the -e argument, this will grab real process names.
        /// </summary>
        [JsonInclude]
        public bool useRealProcessNames = true;



       


        


    }


}
