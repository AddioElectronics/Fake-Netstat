using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace Addio.Antiscam.Fake.Netstat
{
    public class Program
    {

        #region Declarations and Fields


        /// <summary>
        /// Allows user to change multiple settings at once.
        /// </summary>
        public enum Profile { None, Snail, SlowShort, SlowLong, NeverEnding, FastShort, FastLong, FastNeverEnding, VeryFastLong, VeryFastNeverEnding, LightSpeed }

        /// <summary>
        /// The protocol being displayed.
        /// </summary>
        public enum Proto { TCP, UDP, TCPv6, UDPv6, RANDOM };

        /// <summary>
        /// The state being displayed
        /// </summary>
        public enum State { ESTABLISHED, TIME_WAIT, LAST_ACK, CLOSE_WAIT, RANDOM };


        /// <summary>
        /// The configuration for things like how many connections, and how fast it will be displayed.
        /// </summary>
        public static Config config;

        /// <summary>
        /// The netstat arguments that can be set via the command line.
        /// Note: At the moment the only one thats implemented is interval. Feel free to add functionality, I'm done with this project.
        /// </summary>
        static Args arguments = new Args();

        /// <summary>
        /// The random number generator.
        /// </summary>
        static Random random;

        /// <summary>
        /// An array of real TCP connections, that netstat would actually show.
        /// Will be mixed in with the fake connections to make it more believable.
        /// </summary>
        static TcpConnectionInformation[] realConnections;

        /// <summary>
        /// If <see cref="Config.allowRepeatedConnections"/> is false, this is used to keep track of the connections that were used.
        /// </summary>
        static bool[] usedRealConnections, usedCustomConnections;


        /// <summary>
        /// Pre-generated local loopback connections.
        /// </summary>
        static Connection[] loopbackConnections;

        #endregion

        #region Properties

        /// <summary>
        /// The path to the configuration directory.
        /// </summary>
        static string ConfigDirectory
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), "Addio", "Antiscam", "Netstat");
            }
        }

        /// <summary>
        /// The path to the configuration file.
        /// </summary>
        static string ConfigPath
        {
            get
            {
                return Path.Combine(Path.GetTempPath(), "Addio", "Antiscam", "Netstat", "config.json");
            }
        }


        /// <inheritdoc cref="LocalIPAddress"/>
        static string _localIpAddress = null;

        /// <summary>
        /// Gets the current local IP address.
        /// </summary>
        static string LocalIPAddress
        {
            get
            {
                if (_localIpAddress != null) return _localIpAddress;

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        _localIpAddress = ip.ToString();
                        return _localIpAddress;
                    }
                }
                throw new Exception("No network adapters with an IPv4 address in the system!");
            }
        }

        #endregion


        static void Main(string[] args)
        {
            Console.WriteLine(); //New line, this one might not be needed. Can't tell while debugging.

            //Make all characters lower case to make parsing easier.
            Array.ForEach(args, (x) => { x = x.ToLower(); });

            //If the user typed passed a help argument display the help message, and do nothing else.
            if(args.Any((x) => { return Strings.NetstatArgs.help.Contains(x); }))
            {
                Console.WriteLine(Strings.help);
#if DEBUG
                //Stop from closing immediately.
                Console.ReadKey();
#endif
                return;
            }


            Initialize();
            ParseNetstatArguments(args);
            ParseCustomArguments(args);


            //Display the "header"
            Console.WriteLine(Strings.active_connections);
            Console.WriteLine(); //New line
            Console.WriteLine( String.Format(Strings.console_format, Strings.proto, Strings.local_address, Strings.foreign_address, Strings.state));

            //First create the local loopback connections. These are instant
            for (int i = 0; i < config.loopBackConectionCount; i++)
            {
                Console.WriteLine(loopbackConnections[i]);
                Thread.Sleep(1);
            }

            //Now create the regular connections. These take a little while inbetween each connection.
            for (int i = 0; i < config.connectionCount; i++)
            {
                if(config.interval_max > 0)
                Thread.Sleep(arguments.forceInterval > 0 ? arguments.forceInterval * 1000 : random.Next(config.interval_min, config.interval_max));

                //Should we use a custom connection or generate a random one?
                if ((config.custom_connection_formats.Length > 0 || (config.custom_connections.Length > 0 && usedCustomConnections.Any(x => x == false))) && random.NextDouble() < config.custom_chance)
                {
                    if (config.custom_connections.Length > 0 && usedCustomConnections.Any(x => x == false) && random.NextDouble() < 0.5f)
                    {
                        //Pick a random custom command.
                        int index = config.allowRepeatedConnections ? random.Next(0, config.custom_connections.Length - 1) : Array.IndexOf(usedCustomConnections, false);
                        usedCustomConnections[index] = true;

                        Connection con = config.custom_connections[index];

                        if (con.protocol == Proto.RANDOM)
                            con.protocol = RandomProtocol();

                        if (con.state == State.RANDOM)
                            con.state = RandomState();

                        if (con.local_address.Contains(Strings.CustomFormatID.localIP))
                            con.local_address = con.local_address.Replace(Strings.CustomFormatID.localIP, LocalIPAddress + ":" + RandomPort(true));

                        //Display it!
                        Console.WriteLine(con);
                    }
                    else
                    {
                        //Use a custom format to randomize a connection.
                        Connection? con = FormatCustomConnection();

                        //Display it! If its null, then a parsing error happened and will be skipped.
                        if (con != null)
                            Console.WriteLine(con);
                    }
                }
                else
                {
                    //Should we add a real connection? This is an easy way to help make it look more believable, without a bunch of extra work.
                    if (config.useRealConnections && config.custom_connections.Length > 0 && usedCustomConnections.Any(x => x == false) && random.NextDouble() < config.real_chance)
                    {
                        Console.WriteLine(GetRealConnection());
                    }
                    else
                    {
                        //Generate fake connection
                        Console.WriteLine(new Connection(false).ToString());
                    }
                }

            }


#if DEBUG
            //Stop from closing immediately.
            Console.ReadKey();
#endif

        }


        #region Initialization Methods

        /// <summary>
        /// Loads the config json file(or creates it).
        /// And instantiates the random number generator.
        /// Also loads the real TCP connections.
        /// </summary>
        static void Initialize()
        {
            LoadConfig();

            ApplyProfile();

            if (config.random_seed)
                random = new Random();
            else
                random = new Random(config.seed);

            if (config.useRealConnections)
                GetRealTcpConnections();

            if (config.loopBackConectionCount > 0)
                PregenerateLoopbackAddresses();
        }

        /// <summary>
        /// Loads the configuration file if it exists, and creates it if it doesnt.
        /// </summary>
        static void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                if (!Directory.Exists(ConfigDirectory))
                    Directory.CreateDirectory(ConfigDirectory);

                config = new Config();
                SaveConfig();
            }
            else
            {
                string jsonString = File.ReadAllText(ConfigPath);
                config = (Config)JsonSerializer.Deserialize(jsonString, typeof(Config));
            }

            usedCustomConnections = new bool[config.custom_connections.Length];
        }

        /// <summary>
        /// Saves the config file.
        /// </summary>
        static void SaveConfig()
        {
            string jsonString = JsonSerializer.Serialize(config, typeof(Config), new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, jsonString);
        }

        /// <summary>
        /// Changes the settings used by the profile.
        /// </summary>
        static void ApplyProfile()
        {
            if (config.profile <= Profile.None) return;

            if (config.profile > Profile.LightSpeed) config.profile = Profile.LightSpeed;

            switch (config.profile)
            {
                case Profile.Snail:
                    config.connectionCount = 100;
                    config.loopBackConectionCount = 2;
                    config.interval_min = 1000;
                    config.interval_max = 10000;
                    break;
                case Profile.SlowShort:
                    config.connectionCount = 3;
                    config.loopBackConectionCount = 2;
                    config.interval_min = 500;
                    config.interval_max = 1000;
                    break;
                case Profile.SlowLong:
                    config.connectionCount = 100;
                    config.loopBackConectionCount = 10;
                    config.interval_min = 10;
                    config.interval_max = 1000;
                    break;
                case Profile.NeverEnding:
                    config.connectionCount = int.MaxValue;
                    config.loopBackConectionCount = 16;
                    config.interval_min = 10;
                    config.interval_max = 1000;
                    break;
                case Profile.FastShort:
                    config.connectionCount = 3;
                    config.loopBackConectionCount = 2;
                    config.interval_min = 10;
                    config.interval_max = 500;
                    break;
                case Profile.FastLong:
                    config.connectionCount = 100;
                    config.loopBackConectionCount = 10;
                    config.interval_min = 10;
                    config.interval_max = 50;
                    break;             
                case Profile.FastNeverEnding:
                    config.connectionCount = int.MaxValue;
                    config.loopBackConectionCount = 16;
                    config.interval_min = 10;
                    config.interval_max = 100;
                    config.allowRepeatedConnections = true;
                    break;
                case Profile.VeryFastLong:
                    config.connectionCount = 500;
                    config.loopBackConectionCount = 10;
                    config.interval_min = 10;
                    config.interval_max = 100;
                    config.allowRepeatedConnections = true;
                    break;
                case Profile.VeryFastNeverEnding:
                    config.connectionCount = int.MaxValue;
                    config.loopBackConectionCount = 16;
                    config.interval_min = 0;
                    config.interval_max = 10;
                    config.allowRepeatedConnections = true;
                    break;
                case Profile.LightSpeed:
                    config.connectionCount = int.MaxValue;
                    config.loopBackConectionCount = 16;
                    config.interval_min = 0;
                    config.interval_max = 1;
                    config.allowRepeatedConnections = true;
                    break;
            }
        }

        /// <summary>
        /// Gets real TCP connections to mix in with the fake ones.
        /// This makes it a bit easier to make it more believable, 
        /// even though scammers arent smart enough to realize anyways.
        /// </summary>
        static void GetRealTcpConnections()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            realConnections = properties.GetActiveTcpConnections();
            usedRealConnections = new bool[realConnections.Length];
        }

        /// <summary>
        /// Loopback addresses need to be pre-generated because the local address's port will match a different connections foreign address(the loop).
        /// Now a scammer most likely would not know that or realize, but what the hell.
        /// </summary>
        static void PregenerateLoopbackAddresses()
        {
            loopbackConnections = new Connection[(config.loopBackConectionCount % 2) == 0 ? config.loopBackConectionCount: ++config.loopBackConectionCount];

            //Create the connections
            for(int i = 0; i < config.loopBackConectionCount; i++)
            {
                loopbackConnections[i] = new Connection(true);
            }

            //Now create the loop in the ports.
            for (int i = 0; i < config.loopBackConectionCount; i++)
            {
                //Create an offset so its not too pattern like
                int foreignIndex = (config.loopBackConectionCount / 3) + i;
                if (foreignIndex >= config.loopBackConectionCount) foreignIndex -= config.loopBackConectionCount;

                //Split the strings to extract the ports
                string[] localsplit = loopbackConnections[i].local_address.Split(':');
                string[] foreignsplit = loopbackConnections[foreignIndex].foreign_address.Split(':');

                //Set the foreigns port to match the local.
                foreignsplit[foreignsplit.Length - 1] = localsplit[localsplit.Length - 1];

                //Now rejoin the split back onto the ip.
                loopbackConnections[foreignIndex].foreign_address = String.Join(":", foreignsplit);
            }
        }

        #endregion


        #region Argument Parsing Methods

        /// <summary>
        /// (Untested)
        /// Parses the arguemtns passed using Reflection.
        /// </summary>
        /// <param name="args"></param>
        static void ParseNetstatArguments(string[] args)
        {
            //Instead of manually setting each argument, we will use reflection to set them.

            var stringFields = typeof(Strings.NetstatArgs).GetFields();
            var boolFields = arguments.GetType().GetFields();

            for (int i = 0; i < stringFields.Length; i++)
            {
                string value = stringFields[i].GetValue(null) as string;

                if (args.Contains(value))
                {
                    if (value == Strings.NetstatArgs.displayProtocolsOnly)
                    {
                        //This argument is the only one that can be passed a value, so we need to deal with this differently.
                        int index = Array.IndexOf(args, Strings.NetstatArgs.displayProtocolsOnly) + 1;
                        if (args.Length <= index) continue; //Out of range, skip it.
                        Enum.TryParse(args[index], out arguments.displayProtocolsOnly);
                    }
                    else
                    {
                        boolFields[i].SetValue(arguments, true);
                    }
                }
            }

            //This argument is only passed as a number, so we cannot catch it with the reflection code above.
            try
            {
                string intervalString = args.First(x => Regex.IsMatch(x, @"^\d+$"));

                if (intervalString != null && intervalString != "" && ConfirmIntervalArg(args, intervalString))
                {
                    arguments.forceInterval = int.Parse(intervalString);
                }
            }
            catch { }

        }

        /// <summary>
        /// With Netstat, interval was the only argument with a number so it never mattered.
        /// But because we added a custom command which gets passed a number it interferes.
        /// This just makes sure we aren't trying to use a custom argument instead.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="intervalString"></param>
        /// <returns></returns>
        static bool ConfirmIntervalArg(string[] args, string intervalString)
        {
            int index = Array.IndexOf(args, intervalString);

            if (args[index - 1] == Strings.CustomArgs.profile) return false;

            return true;
        }

        /// <summary>
        /// Parses custom arguments users can use to quickly change the config, without going to the file.
        /// </summary>
        static void ParseCustomArguments(string[] args)
        {
            //Open config folder
            if (args.Contains(Strings.CustomArgs.config))
            {
                Process.Start(ConfigDirectory);
            }

            if (args.Contains(Strings.CustomArgs.profile))
            {
                int index = Array.IndexOf(args, Strings.CustomArgs.profile) + 1;

                if (args.Length > index)
                {
                    int newProfile = 0;
                    if (int.TryParse(args[index], out newProfile))
                    {
                        if (newProfile > 10) newProfile = 10;
                        if (newProfile < 0) newProfile = 0;


                        //If the profile isnt 0, then settings have been changed.
                        //So lets load the unchanged settings before we save it.
                        //We will reapply the profile after.
                        if (config.profile != 0)
                        {
                            LoadConfig();
                        }

                        config.profile = (Profile)newProfile;
                        SaveConfig();
                        Initialize();
                    }
                }
            }
        }
        #endregion


        #region Generator Methods


        /// <summary>
        /// Picks a real connection from the list.
        /// </summary>
        /// <returns></returns>
        public static Connection GetRealConnection()
        {
            int index = config.allowRepeatedConnections ? random.Next(0, realConnections.Length - 1) : Array.IndexOf(usedRealConnections, false);
            usedRealConnections[index] = true;

            return new Connection{protocol = Proto.TCP, 
                local_address = realConnections[index].LocalEndPoint.Address.ToString() + ":" + realConnections[index].LocalEndPoint.Port.ToString(), 
                foreign_address = realConnections[index].RemoteEndPoint.Address.ToString() + ":" + realConnections[index].RemoteEndPoint.Port.ToString(), 
                state = RandomState() };

        }


        /// <summary>
        /// Creates a random local IP address.
        /// </summary>
        /// <param name="localLoopback">A local loopback address will always be 127.0.0.1, and will always be displayed at the start of netstat.</param>
        /// <returns>Fake IP address and port.</returns>
        public static string CreateRandomLocalAddress(bool localLoopback)
        {
            if (localLoopback)
                return String.Format(Strings.local_loopback_ip, RandomPort(true));
            else
                return LocalIPAddress + ":" + RandomPort(true);
        }

        /// <summary>
        /// Creates a random foreign IP address.
        /// Does not create domain names.
        /// </summary>
        /// <returns>Fake IP Address</returns>
        public static string CreateRandomForeignIPAddress()
        {
            return String.Format(String.Format(Strings.ip_format, random.Next(1, 255), random.Next(0, 255), random.Next(0, 255), random.Next(0, 255), RandomPort(false)));
        }

        /// <summary>
        /// Creates a random connection using a custom format.
        /// </summary>
        /// <returns>A mostly random fake connection.</returns>
        public static Connection? FormatCustomConnection()
        {
            //Pick a random format
            int index = random.Next(0, config.custom_connection_formats.Length - 1);
            Connection connection = new Connection(config.custom_connection_formats[index]);

            //Protocol
            if (connection.protocol == Proto.RANDOM)
                connection.protocol = RandomProtocol();

            //Local IP
            {
                if (connection.local_address.Contains(Strings.CustomFormatID.localIP))
                    connection.local_address = connection.local_address.Replace(Strings.CustomFormatID.localIP, LocalIPAddress + ":" + RandomPort(true));
            }

            //Foreign Address
            {
                //Port
                if (connection.foreign_address.Contains(Strings.CustomFormatID.port))
                {                  
                    connection.foreign_address = connection.foreign_address.Replace(Strings.CustomFormatID.port, RandomPort(false));
                }

                //Replace numbers
                while (true)
                {
                    int start = connection.foreign_address.IndexOf("{D");
                    if (start == -1) break;
                    int end = connection.foreign_address.IndexOf("}", start);
                    int max;
                    string dn = connection.foreign_address.Substring(start + 2, end - (start + 2));
                    if (!int.TryParse(dn, out max)) return null; //If a parsing error happens we will return null so the connection isn't displayed.

                    connection.foreign_address = connection.foreign_address.Remove(start, end - start + 1);
                    connection.foreign_address = connection.foreign_address.Insert(start, random.Next(0, max).ToString());                    
                }
            }

            //State
                if(connection.state == State.RANDOM)
                    connection.state = RandomState();

            return connection;
        }


        /// <summary>
        /// Generate a random port.
        /// </summary>
        /// <param name="numberOnly">In real netstat a local address will only display a number. This can force a number. </param>
        /// <returns></returns>
        public static string RandomPort(bool numberOnly)
        {
            if (numberOnly) return random.Next(config.port_min, config.port_max).ToString();

            string port;
            int portChance = random.Next();

            //Should the port on the foreign address be https, http, or a number?
            //https is the most likely, and a number is the least likely.
            if (portChance > int.MaxValue / 8)
                port = Strings.https;
            else if (portChance < int.MaxValue / 8 && portChance > int.MaxValue / 100)
                port = Strings.http;
            else
                port = random.Next(config.port_min, config.port_max).ToString();

            return port;
        }

        /// <summary>
        /// Generate a random state. ESTABLISHED, CLOSE_WAIT, etc.
        /// </summary>
        /// <returns>A random state.</returns>
        public static State RandomState()
        {
            return (State)random.Next(0, 3);
        }

        /// <summary>
        /// Generate a random protocol. TCP, UDP, etc...
        /// </summary>
        /// <returns>A random protocl.</returns>
        public static Proto RandomProtocol()
        {
            return (Proto)random.Next(0, 3);
        }


        #endregion



        [Serializable]
        public struct Connection
        {
            [JsonInclude]
            public Program.Proto protocol;

            [JsonInclude]
            public string local_address;

            [JsonInclude]
            public string foreign_address;

            [JsonInclude]
            public Program.State state;


            /// <summary>
            /// Generates a random connection.
            /// </summary>
            /// <param name="localLoopback">Should the connection be a local loop back? (127.0.0.1)</param>
            public Connection (bool localLoopback)
            {
                //I usually only see TCP, so thats all that will return.
                protocol = Proto.TCP;

                local_address = CreateRandomLocalAddress(localLoopback);
                foreign_address = localLoopback ? Environment.MachineName + ":" + RandomPort(true) : CreateRandomForeignIPAddress();

                int stateChance = random.Next();

                //What state should show?
                //ESTABLISHED is the most likely, and LAST_ACK is the least likely.
                if (stateChance > int.MaxValue * 0.4f)
                    state = State.ESTABLISHED;
                else if (stateChance < int.MaxValue * 0.4f && stateChance > int.MaxValue * 0.05f)
                    state = State.TIME_WAIT;
                else
                    state = State.LAST_ACK;
            }

            /// <summary>
            /// Copies another connection, used for custom formats.
            /// </summary>
            public Connection(Connection connection)
            {
                protocol = connection.protocol;
                local_address = connection.local_address;
                foreign_address = connection.foreign_address;
                state = connection.state;
            }


            public override string ToString()
            {
                return String.Format(Strings.console_format, protocol.ToString(), local_address.ToString(), foreign_address.ToString(), state.ToString());
            }
        }       

        /// <summary>
        /// These are parameters that you can pass through netstat.
        /// Doubt a scammer would ever use a parameter, but in the small chance they do,
        /// this can make it look even more real.
        /// Not everything is implemented, and I don't plan to, but thats why this is open source!
        /// </summary>
        public class Args
        {            
            public bool displayAllConnectionsAndPorts = false; //-a            
            public bool displayExecutableInvolved = false; //b            
            public bool displayEthernetStatistics = false; //-e            
            public bool displayFullyQualifiedDomainNames = false; //-f            
            public bool displayInNumericalForm = false; //-n            
            public bool displayOwningProcessId = false; //-o            
            public Proto displayProtocolsOnly = Proto.TCP; //-p proto            
            public bool displayAllConnectionsAndBoundNonListening = false; //-q            
            public bool displayTheRoutingTable = false; //-r            
            public bool displayPerProtocolStatistics = false; //-s            
            public bool displayCurrentConnectionOffloadState = false; //-t            
            public bool displayNetworkDirectConnections = false; //-x            
            public bool displayTcpConnectionTemplateForAll = false; //-y            
            public int forceInterval = -1; //interval

        }


        /// <summary>
        /// A class containing strings that are used multiple times.
        /// Also contains the argument strings.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// The format of the commands in the console.
            /// </summary>
            public const string console_format = "  {0,-7}  {1,-23}  {2,-23}  {3}";


            /// <summary>
            /// All the possible arguments that can be sent to netstat.
            /// </summary>
            public static class NetstatArgs
            {
                public static string displayAllConnectionsAndPorts = "-a";                
                public static string displayExecutableInvolved = "-b";
                public static string displayEthernetStatistics = "-e";
                public static string displayFullyQualifiedDomainNames = "-f";
                public static string displayInNumericalForm = "-n";
                public static string displayOwningProcessId = "-o";
                public static string displayProtocolsOnly = "-p";                
                public static string displayAllConnectionsAndBoundNonListening = "-q";
                public static string displayTheRoutingTable = "-r";
                public static string displayPerProtocolStatistics = "-s";
                public static string displayCurrentConnectionOffloadState = "-t";
                public static string displayNetworkDirectConnections = "-x";
                public static string displayTcpConnectionTemplateForAll = "-y";


                public static string[] help = new string[] { "help", "?", "/?" };
            }

            public static class CustomArgs
            {
                /// <summary>
                /// Opens the config folder
                /// </summary>
                public const string config = "/config";

                /// <summary>
                /// Changes the profile.
                /// </summary>
                public const string profile = "/profile";
            }

            /// <summary>
            /// Strings used to insert strings into the custom connection formats.
            /// </summary>
            public static class CustomFormatID
            {
                /// <summary>
                /// If in a custom command it will be replaced with a random port.
                /// </summary>
                public static string port = "{PORT}";

                /// <summary>
                /// If in a custom command it will be replaced with the local IP.
                /// </summary>
                public static string localIP = "{LOCAL}";

                /// <summary>
                /// If in a custom command it will be replaced with a random state.
                /// </summary>
                public static string state = "{STATE}";

                /// <summary>
                /// If in a custom command it will be replaced with a random protocol.
                /// </summary>
                public static string proto = "{PROTO}";

                /// <summary>
                /// (Not used)
                /// Used for inserting a number. x = the max. 
                /// Example {N0} or {N5}
                /// </summary>
                public static string number = "{Nx}";

                /// <summary>
                /// (Not used)
                /// Used for inserting a number. x = the digit count. 
                /// Example {D2} = 14 (a 2 digit number)
                /// (Not Implemented)
                /// </summary>
                public static string digits = "{Dx}";
            }

            //public static Dictionary<string, string> argumentStrings = new Dictionary<string, string>()
            //{
            //    { "displayAllConnectionsAndPorts" , "-a"},
            //    { "displayExecutableInvolved" , "b"},
            //    { "displayEthernetStatistics" , "-e"},
            //    { "displayFullyQualifiedDomainNames" , "-f"},
            //    { "displayInNumericalForm" , "-n"},
            //    { "displayOwningProcessId" , "-o"},
            //    { "displayProtocolsOnly" , "-p"},
            //    { "displayAllConnectionsAndBoundNonListening" , "-q"},
            //    { "displayTheRoutingTable" , "-r"},
            //    { "displayPerProtocolStatistics" , "-s"},
            //    { "displayCurrentConnectionOffloadState" , "-t"},
            //    { "displayNetworkDirectConnections" , "-x"},
            //    { "displayTcpConnectionTemplateForAll" , "-y"}
            //};

            public const string active_connections = "Active Connections";

            public const string tcp = "TCP";
            public const string proto = "Proto";
            public const string local_address = "Local Address";
            public const string foreign_address = "Foreign Address";
            public const string state = "State";



            public const string established = "ESTABLISHED";
            public const string time_wait = "TIME_WAIT";
            public const string last_ack = "LAST_ACK";


            public const string ip_format = "{0}.{1}.{2}.{3}:{4}";

            public const string http = "http";
            public const string https = "https";

            public const string local_loopback_ip = "127.0.0.1:{0}";


            public const string help = @"Displays protocol statistics and current TCP/IP network connections.

NETSTAT [-a] [-b] [-e] [-f] [-n] [-o] [-p proto] [-r] [-s] [-x] [-t] [interval]

  -a            Displays all connections and listening ports.
  -b            Displays the executable involved in creating each connection or
                listening port. In some cases well-known executables host
                multiple independent components, and in these cases the
                sequence of components involved in creating the connection
                or listening port is displayed. In this case the executable
                name is in [] at the bottom, on top is the component it called,
                and so forth until TCP/IP was reached. Note that this option
                can be time-consuming and will fail unless you have sufficient
                permissions.
  -e            Displays Ethernet statistics. This may be combined with the -s
                option.
  -f            Displays Fully Qualified Domain Names (FQDN) for foreign
                addresses.
  -n            Displays addresses and port numbers in numerical form.
  -o            Displays the owning process ID associated with each connection.
  -p proto      Shows connections for the protocol specified by proto; proto
                may be any of: TCP, UDP, TCPv6, or UDPv6.  If used with the -s
                option to display per-protocol statistics, proto may be any of:
                IP, IPv6, ICMP, ICMPv6, TCP, TCPv6, UDP, or UDPv6.
  -q            Displays all connections, listening ports, and bound
                nonlistening TCP ports. Bound nonlistening ports may or may not
                be associated with an active connection.
  -r            Displays the routing table.
  -s            Displays per-protocol statistics.  By default, statistics are
                shown for IP, IPv6, ICMP, ICMPv6, TCP, TCPv6, UDP, and UDPv6;
                the -p option may be used to specify a subset of the default.
  -t            Displays the current connection offload state.
  -x            Displays NetworkDirect connections, listeners, and shared
                endpoints.
  -y            Displays the TCP connection template for all connections.
                Cannot be combined with the other options.
  interval      Redisplays selected statistics, pausing interval seconds
                between each display.  Press CTRL+C to stop redisplaying
                statistics.  If omitted, netstat will print the current
                configuration information once.";

        }


    }
}
