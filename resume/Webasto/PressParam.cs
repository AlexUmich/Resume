using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConsoleApplication {
    class Program {
        private static List<TcpClient> clients = new List<TcpClient>();
        public static void RemoveClient(TcpClient client) {
            clients.Remove(client);
        }

        public static int GetClientCount() {
            return clients.Count;
        }

        public static string GetLocalIPAddress() {
            if(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true) {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach(var ip in host.AddressList) {
                    if(ip.AddressFamily == AddressFamily.InterNetwork) {
                        return ip.ToString();
                    }
                }
            }
            return "No Internet Connection";
        }

        static void Main(string[] args) {
            int count = 0;
            int stop = 0;
            DateTime startDateTime = DateTime.Now;
            string address = GetLocalIPAddress();
            Console.WriteLine(GetLocalIPAddress());
            while(address == "No Internet Connection") {
                Console.WriteLine("Unable To Obtain Correct/An IP Address. Expected IP Not Found, Or No Internet Connection Detected");
                Console.WriteLine("Retrying in 10 seconds\n");
                Thread.Sleep(10000);
                Console.WriteLine(address);
            }
            IPAddress ipAddress = IPAddress.Parse(address);
            TcpListener serverSocket = new TcpListener(ipAddress, 585);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            //while(startDateTime.ToString("MM.dd.yyyy") == DateTime.Now.AddHours(-6).AddMinutes(-30).ToString("MM.dd.yyyy")) {
            while(stop == 0) {
                try {
                    count = 0;
                    address = GetLocalIPAddress();
                    while(address == "No Internet Connection") {
                        Console.WriteLine("Unable To Obtain Local IP Address. No Internet Connection Detected");
                        Console.WriteLine("Retrying in 10 seconds");
                        Thread.Sleep(10000);
                        address = GetLocalIPAddress();
                    }
                    Console.WriteLine(GetClientCount() + " clients connected");
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("Waiting for a connection...");
                    clientSocket = serverSocket.AcceptTcpClient();
                    clients.Add(clientSocket);
                    handleClient client = new handleClient();
                    client.startClient(clientSocket);
                    while(GetClientCount() == 12) {
                        if(count < 1) {
                            Console.WriteLine(GetClientCount() + " clients connected");
                            Console.WriteLine("Expected number of clients reached.");
                            count++;
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine("In Main, Error Occured: " + e.ToString());
                }
            }
            clientSocket.Close();
            serverSocket.Stop();
            Environment.Exit(0);
        }
    }

    public class handleClient {
        TcpClient clientSocket;
        public void startClient(TcpClient inClientSocket) {
            this.clientSocket = inClientSocket;
            Thread recieveThread = new Thread(recieve);
            recieveThread.Start();
        }

        public static string ReturnLine(string path, Encoding encoding, string num_first_digit, string num_second_digit) {
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            bool found1 = false;
            bool found2 = false;
            bool found3 = false;
            int count = 0;
            using(FileStream fs = new FileStream(path, FileMode.Open)) {
                Int64 endPosition = fs.Length / sizeOfChar;
                for(Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar) {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);
                    if(count == 20) {
                        byte[] returnBuffer = new byte[fs.Length - fs.Position];
                        fs.Read(returnBuffer, 0, returnBuffer.Length);
                        return encoding.GetString(returnBuffer);
                    }
                    if(found3 == true) {
                        count++;
                    }
                    if(found2 == true) {
                        if(encoding.GetString(buffer) == "P") {
                            found3 = true;
                        }
                        found2 = false;
                    }
                    if(found1 == true) {
                        if(encoding.GetString(buffer) == num_first_digit) {
                            found2 = true;
                        }
                        found1 = false;
                    }
                    if((encoding.GetString(buffer) == num_second_digit) && (count == 0)) {
                        found1 = true;
                    }
                }
                return "error";
            }
        }

        public static string ReturnLine2(string path, Encoding encoding) {
            int sizeOfChar = encoding.GetByteCount("\n");
            byte[] buffer = encoding.GetBytes("\n");
            int count = 0;
            using(FileStream fs = new FileStream(path, FileMode.Open)) {
                Int64 endPosition = fs.Length / sizeOfChar;
                for(Int64 position = sizeOfChar; position < endPosition; position += sizeOfChar) {
                    fs.Seek(-position, SeekOrigin.End);
                    fs.Read(buffer, 0, buffer.Length);
                    if(encoding.GetString(buffer) == "\n") {
                        if(count != 0) {
                            byte[] returnBuffer = new byte[fs.Length - fs.Position];
                            fs.Read(returnBuffer, 0, returnBuffer.Length);
                            return encoding.GetString(returnBuffer);
                        }
                        count++;
                    }
                }
                return "error";
            }
        }

        private void recieve() {
            var csv = new StringBuilder();
            var csv00 = new StringBuilder();
            var csv99 = new StringBuilder();
            byte[] bytes = new byte[1024];
            DateTime currentDateTime = new DateTime();
            string data = null;
            string data2 = null;
            string path = null;
            int bytesRec = 0;
            string[] labels00 = {"Date,Forming_Time,Deg1_Qty,Deg1_Press,Deg1_Speed,Deg1_Decomp,Deg1_Offset,Deg1_OnOff,Deg1_Delay,Deg1_Duration,Deg1_C1_RetTime,Deg1_C23_RetTime,VacRel_En,Deg1_VacRel_Time,Deg2_OnOff,Deg2_Delay,Deg2_Duration,Deg2_C1_RetTime,Deg2_C23_RetTime,VacRel_En,Deg2_VacRel_Time,Subs_PreHeat_En,Subs_PreHeat_Timer,Subs_PreHeat_BlowOffT,HeatSetPt_Up_1,HeatSetPt_Up_2,HeatSetPt_Up_3,HeatSetPt_Up_4,HeatSetPt_Up_5,HeatSetPt_Up_6,HeatSetPt_Up_7,HeatSetPt_Up_8,HeatSetPt_Low_1,HeatSetPt_Low_2,HeatSetPt_Low_3,HeatSetPt_Low_4,HeatSetPt_Low_5,HeatSetPt_Low_6,HeatSetPt_Low_7,HeatSetPt_Low_8,Sandwich_W,Sandwich_L"};
            string[] labels99 = {"Date,Press_ID,ToolClosed_Pos,HeatOfs_Up_1,HeatOfs_Up_2,HeatOfs_Up_3,HeatOfs_Up_4,HeatOfs_Up_5,HeatOfs_Up_6,HeatOfs_Up_7,HeatOfs_Up_8,HeatOfs_Low_1,HeatOfs_Low_2,HeatOfs_Low_3,HeatOfs_Low_4,HeatOfs_Low_5,HeatOfs_Low_6,HeatOfs_Low_7,HeatOfs_Low_8,DropOfs_X,DropOfs_Y,DropOfs_Z,HandOverOfs_X,HandOverOfs_Y"};
            for(int i = 0; i < 3; i++) {
                csv00.AppendLine(labels00[i]);
            }
            for(int i = 0; i < 3; i++) {
                csv99.AppendLine(labels99[i]);
            }
            while(true) {
                try {
                    Array.Clear(bytes, 0, bytes.Length);
                    NetworkStream networkStream = clientSocket.GetStream();
                    bytesRec = networkStream.Read(bytes, 0, bytes.Length);
                    currentDateTime = DateTime.Now;
                    data = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    data += ",";
                    data2 = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if(data2.Substring(6, 1) == "R") {
                        path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\PHC_Recipe_Management\\RMS_T" + data2.Substring(1, 2) + "_Tool.csv";
                        string final_data = ReturnLine2(path, Encoding.ASCII);
                        string num1 = data2.Substring(9, 1);
                        string num2 = data2.Substring(10, 1);
                        path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\PHC_Recipe_Management\\RMS_T" + data2.Substring(1, 2) + "_Press.csv";
                        string part2 = ReturnLine(path, Encoding.ASCII, num1, num2);
                        string final_data2 = part2.Substring(0, 20);
                        final_data2 += part2.Substring(24);
                        if(final_data == "error") {
                            byte[] written = Encoding.ASCII.GetBytes("Error, no Tool data for Press 00 found.");
                            networkStream.Write(written, 0, written.Length);
                        }
                        else {
                            int spot = data.IndexOf("\n");
                            final_data += data.Substring(0, spot);
                            byte[] written = Encoding.ASCII.GetBytes(final_data);
                            networkStream.Write(written, 0, written.Length);
                        }
                        if(part2 == "error") {
                            byte[] written = Encoding.ASCII.GetBytes("Error, no Tool data for Press " + data2.Substring(1, 2) + "found.");
                            networkStream.Write(written, 0, written.Length);
                        }
                        else {
                            int spot = data.IndexOf("\n");
                            final_data += data.Substring(0, spot);
                            byte[] written = Encoding.ASCII.GetBytes(final_data2);
                            networkStream.Write(written, 0, written.Length);
                        }
                    }
                    if(data2.Substring(4, 2) == "00") {
                        data += data2.Substring(6);
                        path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\PHC_Recipe_Management\\RMS_T" + data2.Substring(1, 2) + "_Tool.csv";
                        if(!File.Exists(path)) {
                            File.AppendAllText(path, csv00.ToString());
                        }
                    }
                    else {
                        data += "P";
                        data += data2.Substring(4, 2);
                        data += ",";
                        data += data2.Substring(6);
                        path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\PHC_Recipe_Management\\RMS_T" + data2.Substring(1, 2) + "_Press.csv";
                        if(!File.Exists(path)) {
                            File.AppendAllText(path, csv99.ToString());
                        }
                    }
                    Console.WriteLine("Text received: {0}", data);
                    csv.AppendLine(data);
                    File.AppendAllText(path, csv.ToString());
                    csv.Clear();
                }
                catch(Exception e) {
                    Console.WriteLine("In Thread, Error Occured: " + e.ToString());
                    if(e.ToString().Substring(0, 73) == "System.IO.IOException: Unable to read data from the transport connection:") {
                        clientSocket.Close();
                        Program.RemoveClient(clientSocket);
                        return;
                    }
                    else if(e.ToString().Substring(0, 71) == "System.IO.DirectoryNotFoundException: Could not find a part of the path") {
                        Console.WriteLine("Note: Cannot access O-Drive or the target folder within it.");
                    }
                    else if(e.ToString().Substring(0, 57) == "System.IO.IOException: The process cannot access the file") {
                        Console.WriteLine("Note: Target CSV File In Use.");
                    }
                    else {
                        Console.WriteLine("Unexpected Error Detected. Program will pause for 10 seconds");
                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}
