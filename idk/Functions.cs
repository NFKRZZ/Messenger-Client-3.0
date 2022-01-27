using Messenger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MessengerClient
{

    public class Functions
    {

        [DllImport("DONUT.dll", EntryPoint = "main", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern int main();

        public static bool isSharing = true;
        public static bool windowNotOpen = true;
        static bool waiting = false;
        static bool first = true;
        public static Form window = new Form();
        static PictureBox pictureBox = new PictureBox();
        static bool sizeChange = false;
        //  static ControlEventHandler client;
        //  public event EventHandler ClientSizeChanged;
        public static List<string> splitWB(string b)
        {
            List<string> a = new List<string>();
            int length = b.Length;
            int chunkAmount = length / 2;
            for (int i = 0; i < length; i += 5)
            {
                a.Add(b.Substring(i, Math.Min(5, length - i)));
            }

            return a;
        }
        public static void PrintWb(List<string> w)
        {
            int i = 0;
            foreach (string wow in w)
            {
                if (i % 2 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(wow);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(wow);
                }
                i++;
            }
            Console.WriteLine();
            Console.ForegroundColor = Program.currentColor;
        }
        public static void printRGB(List<string> j)
        {
            List<colors> color = Enum.GetValues(typeof(colors)).Cast<colors>().ToList();
            Random random = new Random();
            int i = random.Next(0, 6);
            foreach (string wow in j)
            {
                if (i > 5 || i < 0)
                {
                    i = 0;
                }
                Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), color[i].ToString(), false);
                Console.Write(wow);
                i++;
                if (i == 5)
                {
                    i = 0;
                }
            }
            random = null;
            i = 0;
            Console.WriteLine();
            Console.ForegroundColor = Program.currentColor;
        }
        public static void changeBackground(string l)
        {
            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), l, true);
            Console.Clear();
            foreach (Packet p in MessengerClient.Program.logs)
            {
                messageType type = p.messageType;
                if (type == messageType.STRING)
                {
                    string color;
                    ConsoleColor c;
                    string str = p.message;
                    if (p.useHex)
                    {
                        color = p.hexColor;
                    }
                    else
                    {
                        c = p.color;
                        Console.ForegroundColor = c;
                        if (Console.ForegroundColor == ConsoleColor.Black)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.WriteLine(str);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = Program.currentColor;
                        }
                        else if (str.Contains("-rainbow"))
                        {
                            str = str.Replace("-rainbow", "");
                            str += "";
                            List<string> j = Functions.splitWB(str);
                            Functions.printRGB(j);
                        }
                        else if (str.Contains("-wb"))
                        {
                            str = str.Replace("-wb", "");
                            List<string> k = Functions.splitWB(str);
                            Functions.PrintWb(k);
                        }
                        else
                        {
                            Console.WriteLine(str);
                            Console.ForegroundColor = Program.currentColor;
                        }
                    }
                }
                else if (type == messageType.FILE)
                {

                }
                else if (type == messageType.IMAGE)
                {

                }
                else if (type == messageType.VIDEO)
                {

                }
            }
        }
        public static string RandomName()
        {
            return "RaviIsAGod";
        }
        public static void ScreenShare()
        {
            int i = 0;
            NetworkStream stream = Program.host.GetStream();
            string a;
            Stopwatch stopwatch = new Stopwatch();
            while (isSharing)
            {
                stopwatch.Start();
                // Thread.Sleep(1);
                MemoryStream ms = new MemoryStream();
                Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
                Graphics gScreeny = Graphics.FromImage(screenshot);
                gScreeny.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                screenshot.Save("pic.jpeg", ImageFormat.Jpeg);
              //  Console.WriteLine("CREATION OF SCREENSHOT TIME: " + stopwatch.ElapsedMilliseconds);
                screenshot.Save(ms, ImageFormat.Jpeg);
                byte[] lol = compressByte(ms.ToArray());
                Packet p = new Packet(lol, messageType.IMAGE);
               // Console.WriteLine("Image Size"+lol.LongLength);
                ProtoBuf.Serializer.SerializeWithLengthPrefix(stream, p, ProtoBuf.PrefixStyle.Base128);
                stopwatch.Stop();
               // Console.WriteLine("ELAPSED TIME FINAL: + " + stopwatch.ElapsedMilliseconds);
                stopwatch.Reset();
                i++;
                //https://stackoverflow.com/questions/749964/sending-and-receiving-an-image-over-sockets-with-c-sharp
                ms.Dispose();
                ms = null;
                p = null;

                //Console.WriteLine("GANG IMAGE SENT");
            }
            Console.WriteLine("aborted");

            waiting = true;

            while (waiting)
            {
                if (isSharing)
                {
                    break;
                }
            }
            ScreenShare();

        }
        public static void ScreenShareWindow(string[] data)
        {
             Console.WriteLine("image window opened");

            pictureBox.Dock = DockStyle.Fill;
            window.Controls.Add(pictureBox);
            window.ClientSizeChanged += new EventHandler(ClientSizeChanged);
            // window.Visible = false;
            window.ShowDialog();


            // windowNotOpen = false;



        }
        private static void ClientSizeChanged(Object sender, EventArgs e)
        {
           // Console.WriteLine("hello");
            sizeChange = true;
        }
        public static void ScreenImage(byte[] iData)
        {
            if (true)
            {
                if (first == false)
                {
                   //   Console.WriteLine("Not First");
                    //pictureBox.Image=null;
                }
                try
                {
                    byte[] data = decompressByte(iData);
                    var ms = new MemoryStream(data);
                    Bitmap image = new Bitmap(ms);
                    // Console.WriteLine("Image applied tasdassdo screen");
                    Graphics gScreeny = Graphics.FromImage(image);
                    image.Save("lola.jpg", ImageFormat.Jpeg);
                    //Console.WriteLine("Set new pic");
                    pictureBox.Image = image;
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    first = false;
                    image = null;
                    ms.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e);
                }
            }
        }
        public static void DisplayDonut()
        {
            float A = 0, B = 0;
            double i, j;
            int k;
            float[] z = new float[1760];
            char[] b = new char[1760];
            Console.Write("\x1b[2J");
            for (; ; )
            {
                // MemSet(b, 32, 1760);
                // MemSet(z, 0, 7040);
                for (j = 0; j < 6.28; j += 0.07)
                {
                    for (i = 0; i < 6.28; i += 0.02)
                    {
                        double c = Math.Sin(i);
                        double d = Math.Cos(j);
                        double e = Math.Sin(A);
                        double f = Math.Sin(j);
                        double g = Math.Cos(A);
                        double h = d + 2;
                        double D = 1 / (c * h * e + f * g + 5);
                        double l = Math.Cos(i);
                        double m = Math.Cos(B);
                        double n = Math.Sin(B);
                        double t = c * h * g - f * e;
                        int x = (int)(40 + 30 * D * (l * h * m - t * n));
                        int y = (int)(12 + 15 * D * (l * h * n + t * m));
                        int o = x + 80 * y;
                        int N = (int)(8 * ((f * e - c * d * g) * m - c * d * e - f * g - l * d * n));
                        if (22 > y && y > 0 && x > 0 && 80 > x && D > z[o])
                        {
                            z[o] = (float)D;
                            b[o] = ".,-~:;=!*#$@"[N > 0 ? N : 0];
                        }
                    }
                }
                Console.Write("\x1b[H");
                for (k = 0; k < 1761; k++)
                {
                    Console.Write(k % 80 == 0 ? b[k] : 10);
                    A += (float)0.00004;
                    B += (float)0.00002;
                }
                //usleep(30000);
            }
            // return 0;
        }
        public static byte[] compressByte(byte[] data)
        {
            using(var compressedStream = new MemoryStream())
                using(var gs = new GZipStream(compressedStream,CompressionMode.Compress))
                {
                    gs.Write(data, 0, data.Length);
                    gs.Close();
                    return compressedStream.ToArray();
                }
        }
        public static byte[] decompressByte(byte[] data)
        {
            using (var compressed = new MemoryStream(data))
                using(var gs = new GZipStream(compressed,CompressionMode.Decompress))
                    using(var rs = new MemoryStream())
                    {
                        gs.CopyTo(rs);
                        return rs.ToArray();
                    }
        }


    }
    public enum colors
    {
        Red = 0,
        Yellow = 1,
        Green = 2,
        Blue = 3,
        Magenta = 4,
        DarkMagenta = 5
    }
}
