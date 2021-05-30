using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Pastel;
using System.Windows.Forms;
using Messenger;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace MessengerClient
{
    class Program
    {
        public static TcpClient host = new TcpClient();
        static int port = 22581;
        static string username;
        public static ConsoleColor currentColor = ConsoleColor.Gray;
        public static List<Packet> logs = new List<Packet>();
        public static List<Thread> runningTasks = new List<Thread>();
        static Thread MainT;
        public static bool screenshareOn = false;
        static bool usingPastel = false;
        static bool pastelFirst = false;
        static bool changedPastel = false;
        static string pastelColor;
        static readonly string colorInfo = "Black, Blue, Cyan, DarkBlue, DarkCyan, DarkGray, DarkGreen, DarkMagenta, DarkRed, DarkYellow, Gray, Green, Magenta, Red, White, Yellow";
        static void Main(string[] args)
        {
            Connect();
        }
        static void Connect()
        {
            try
            {
                host = new TcpClient();
                Console.WriteLine("Type in server ip");
                IPAddress IP = IPAddress.Parse(Console.ReadLine());
                host.Connect(IP, port);
                Console.WriteLine("Connected");
                Console.WriteLine("Type in username");
                username = Console.ReadLine();
                if (username.Equals(""))
                {
                    username = Functions.RandomName();
                }
                Thread input = new Thread(() => Input());
                MainT = input;
                Console.Clear();
                input.Start();
                Thread receiveTask = new Thread(() => Receive());
                receiveTask.Start();
                Thread checker = new Thread(() => Checker());
                checker.Start();
                runningTasks.Add(checker);
                runningTasks.Add(input);
                runningTasks.Add(receiveTask);
            }
            catch (Exception e)
            {
                if (!(e is System.AggregateException))
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Connection Error");
                    Console.WriteLine("Retrying");
                    Connect();
                }
                else
                {
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(2000);
                    Console.WriteLine("SERVER CLOSED");
                    try
                    {
                        host = null;
                        foreach (Thread s in runningTasks)
                        {
                            s.Abort();
                        }
                    }
                    catch (Exception a)
                    {
                        throw new Exception("problem aborting thread");
                    }
                    Console.Clear();
                    Connect();
                }
            }

        }
        public static string getTime()
        {
            string time = "[" + DateTime.Now.ToString("hh:mm:ss") + "]: ";
            return time;
        }
        static void Input()
        {
            char delemiter = ' ';
            Thread ssTask = null;
            while (true)
            {
                try
                {
                    string a = Console.ReadLine();
                   ClearLine();
                    if (a.Contains("-c") && !a.Contains("-bc"))
                    {
                        if (a.Contains("list"))
                        {
                            Console.WriteLine(colorInfo);
                        }
                        else
                        {
                            string[] aa = a.Split(delemiter);
                            string lol = aa[1];
                            changeColor(lol);
                        }

                    }
                    else if (a.Contains("-ss") && !a.Contains("cancel"))
                    {
                        Functions.isSharing = true;
                        ssTask = new Thread(() => Functions.ScreenShare());
                        ssTask.Start();
                        runningTasks.Add(ssTask);

                    }
                    else if (a.Contains("-ss") && a.Contains("cancel"))
                    {
                        if (ssTask != null)
                        {
                            Console.WriteLine("task aborted");
                            Functions.isSharing = false;
                            ssTask.Abort();
                            runningTasks.Remove(ssTask);
                            ssTask = null;
                        }
                        else
                        {

                        }
                    }
                    else if (a.Contains("-help"))
                    {
                        if (Console.BackgroundColor == ConsoleColor.DarkMagenta)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        }
                        Console.WriteLine("Commands for client are:\n-c [color] changes color of text\n-f [int] changes the font size\n"
                           + "-fs [file path] sends a file to all connected user\n-draw opens up the drawing app\n-rainbow [text] sends text in rainbow colors"
                           + "\n-bc [color] changes the background color of the window");
                        Console.ForegroundColor = currentColor;
                    }
                    else if (a.Contains("-nw"))
                    {
                        var window = new Form();
                        window.Show();
                    }
                    else if (a.Contains("-f") && !a.Contains("-fs"))
                    {

                        string[] aa = a.Split(delemiter);
                        int lol = int.Parse(aa[1]);
                        changeFont(lol);
                    }
                    else if (a.Contains("-fs"))
                    {
                        string[] aa = a.Split(delemiter);
                        string ll = aa[1];
                        fileSend(ll);
                    }
                    else if (a.Contains("-draw"))
                    {
                        Draw();
                    }
                    else if (a.Contains("-rainbow"))
                    {
                        Send(a);
                    }
                    else if (a.Contains("-bc"))
                    {
                        string[] l = a.Split(delemiter);
                        Functions.changeBackground(l[1]);
                    }
                    else
                    {
                        Send(a);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error wrong command" + e);
                    Input();
                }
            }
        }
        static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new String(' ', Console.BufferWidth));
            Console.Write("\r");
            if (usingPastel&&!pastelFirst)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 0);
                Console.WriteLine("First");
            }
            else if(usingPastel&&pastelFirst)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                pastelFirst = false;
                Console.WriteLine("Second");
            }
            else if(!usingPastel)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
              //  Console.WriteLine("thirds");
            }
        }
        static void changeColor(string a)
        {
            try
            {
                if (!a.Contains("#"))
                {
                    usingPastel = false;
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), a, true);
                    currentColor = Console.ForegroundColor;
                }
                else
                {
                    usingPastel = true;
                    if (!pastelFirst)
                    {
                        pastelFirst = true;
                    }
                    pastelColor = a;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Color Does Not Exist, type -c list for all possible colors");
                Input();
            }
        }
        static void changeFont(int lol)
        {

        }
        static void Send(string a)
        {
            Packet packet = null;
            if (usingPastel)
            {
                packet = new Packet(getTime()+username+": "+a, messageType.STRING, pastelColor, true);
            }
            else if (!usingPastel)
            {
                packet = new Packet(getTime()+username+": "+a, messageType.STRING, Console.ForegroundColor, false);
            }
            NetworkStream stream = host.GetStream();
            ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, packet, ProtoBuf.PrefixStyle.Base128);
        }
        static void Receive()
        {
            try
            {
                NetworkStream stream = host.GetStream();
                byte[] data = new byte[24000000];
                while (true)
                {   
                    Packet  p = ProtoBuf.Serializer.DeserializeWithLengthPrefix<Packet>(stream, ProtoBuf.PrefixStyle.Base128);   
                    messageType type = p.messageType;
                    //Console.WriteLine("This is a "+type);
                    if (type == messageType.STRING)
                    {
                        logs.Add(p);
                        if (p.useHex is false)
                        {
                            Console.ForegroundColor = p.color;
                            if (Console.ForegroundColor == ConsoleColor.Black)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.WriteLine(p.message);
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.ForegroundColor = currentColor;
                            }
                            else if (p.message.Contains("-rainbow"))
                            {
                                p.message = p.message.Replace("-rainbow", "");
                                p.message += ",!";
                                List<string> j = Functions.splitWB(p.message);
                                Functions.printRGB(j);
                            }
                            else if (p.message.Contains("-wb"))
                            {
                                p.message = p.message.Replace("-wb", "");
                                List<string> k = Functions.splitWB(p.message);
                                Functions.PrintWb(k);
                            }
                            else
                            {
                                Console.WriteLine(p.message);
                                Console.ForegroundColor = currentColor;
                            }
                        }
                        else
                        {
                            if (p.message.Contains("-rainbow"))
                            {
                                p.message = p.message.Replace("-rainbow", "");
                                p.message += ",!";
                                List<string> j = Functions.splitWB(p.message);
                                Functions.printRGB(j);
                            }
                            else if (p.message.Contains("-wb"))
                            {
                                p.message = p.message.Replace("-wb", "");
                                List<string> k = Functions.splitWB(p.message);
                                Functions.PrintWb(k);
                            }
                            else
                            {
                                Console.WriteLine(p.message.Pastel(p.hexColor));
                            }
                        }
                    }
                    else if (type == messageType.FILE)
                    {

                    }
                    else if (type == messageType.IMAGE)
                    {
                        if (!screenshareOn)
                        {
                            Thread window = new Thread(() => Functions.ScreenShareWindow(new string[2]));
                            window.Start();
                            runningTasks.Add(window);
                            screenshareOn = true;
                        }
                        Functions.ScreenImage(p.image);
                     
                    }
                    else if (type == messageType.VIDEO)
                    {

                    }
                    else
                    {
                        Console.WriteLine("Unknown type "+type);
                    }
                }
            }
            catch (Exception e)
            {
               // Receive();
                   Console.WriteLine(e.ToString());
            }
        }
        static void fileSend(string filePath)
        {
            Console.WriteLine("shit");
            string filename = filePath;
            FileStream fs = new FileStream(filename, FileMode.Open);
            host.SendTimeout = 60000;
            host.ReceiveTimeout = 60000;
            NetworkStream stream = host.GetStream();

            //  stream.Write(Encoding.ASCII.GetBytes(fs), 0, Encoding.ASCII.GetBytes(fs).Length);
            Console.WriteLine(filePath);

        }
        static void Checker()
        {
           // Console.WriteLine("Checker initiated");
            while(true)
            {
                if (host.Connected)
                {
                  //  Console.Write("\r" + getTime() + "CHECKING");
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine("BROKE");
            if (!host.Connected)
            {
                Console.Clear();
                Console.WriteLine("Lost Connection To the server, returning to main menu in 3 seconds".Pastel("#5916c4"));
                Thread.Sleep(3000);
                Restart();
            }
        }
        static void Restart()
        {
            foreach(Thread s in runningTasks)
            {
                if(!s.Equals(MainT))
                {
                    s.Abort();
                    runningTasks.Remove(s);
                }
                host = null;
                host = new TcpClient();
                currentColor = ConsoleColor.Gray;
                screenshareOn = false;
                usingPastel = false;
                pastelFirst = false;
                pastelColor = string.Empty;
                Console.Clear();    
                Connect();
            }

        }
        static void Draw()
        {
            bool draw = true;
            while (draw)
            {
                draw = false;
            }
            Input();
        }
    }
}
