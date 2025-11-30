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

        public static void Log(string logMessage, TextWriter w) {
            w.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------");
            w.WriteLine("Log Entry @ " + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + ":");
            w.WriteLine(logMessage);
        }

        public static void Log2(string logMessage, TextWriter w) {
            w.WriteLine(logMessage);
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
            using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                Log("Program Start", log_w);
            }
            Console.WriteLine("Program Start");
            string combined = null;
            int count = 0;
            DateTime startDateTime = DateTime.Now;
            string address = GetLocalIPAddress();
            while(address == "No Internet Connection") {
            //while(address != "10.137.194.165") {
            //while(address != "192.168.1.252") {
                using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                    Log("No Internet/Incorrect Network.\nUnable To Obtain Correct Local IP Address. Or No Internet Connection Detected\nRetrying in 10 seconds\n", log_w);
                }
                Console.WriteLine("Unable To Obtain Correct/An IP Address. Expected IP Not Found, Or No Internet Connection Detected");
                Console.WriteLine("Retrying in 10 seconds\n");
                Thread.Sleep(10000);
                using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                    combined = "Address/Status: " + address;
                    Log2(combined, log_w);
                }
                Console.WriteLine(address);
            }
            IPAddress ipAddress = IPAddress.Parse(address);
            TcpListener serverSocket = new TcpListener(ipAddress, 9);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            int stop = 0;
            //while(startDateTime.ToString("MM.dd.yyyy") == DateTime.Now.AddHours(-6).AddMinutes(-30).ToString("MM.dd.yyyy")) {
            while(stop == 0) {
                try {
                    count = 0;
                    address = GetLocalIPAddress();
                    while(address == "No Internet Connection") {
                        //while(address != "10.137.194.165") {
                        //while(address != "192.168.1.252") {
                        using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                            Log("No Internet.\nUnable To Obtain Local IP Address. No Internet Connection Detected.\nRetrying in 10 seconds...\n", log_w);
                        }
                        Console.WriteLine("Unable To Obtain Local IP Address. No Internet Connection Detected");
                        Console.WriteLine("Retrying in 10 seconds");
                        Thread.Sleep(10000);
                        address = GetLocalIPAddress();
                    }
                    using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                        combined = GetClientCount() + " clients connected";
                        Log(combined, log_w);
                    }
                    Console.WriteLine(GetClientCount() + " clients connected");
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("Waiting for a connection...");
                    //Console.WriteLine(address);
                    using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                        Log("Waiting for a connection...", log_w);
                    }
                    clientSocket = serverSocket.AcceptTcpClient();
                    string address2 = ((IPEndPoint)clientSocket.Client.RemoteEndPoint).Address.ToString();
                    Console.WriteLine(address2);
                    if(address2 == "192.168.1.2" || address2 == "10.137.232.3" || address2 == "10.137.232.4" || address2 == "10.137.232.5" || address2 == "10.137.232.6") {
                        clients.Add(clientSocket);
                        handleClient client = new handleClient();
                        client.startClient(clientSocket);
                        //Console.WriteLine(GetClientCount() + " clients connected");
                    }
                    else {
                        Console.WriteLine("Warning. Unknown client attempted to make a connection.");
                        using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                            Log("Warning. Unknown client attempted to make a connection.", log_w);
                        }
                    }
                    while(GetClientCount() == 4) {
                        if(count < 1) {
                            using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                                combined = GetClientCount() + " clients connected";
                                Log(combined, log_w);
                                Log2("Expected number of clients reached.", log_w);
                            }
                            Console.WriteLine(GetClientCount() + " clients connected");
                            Console.WriteLine("Expected number of clients reached.");
                            count++;
                        }
                        if(DateTime.Now.ToString("HH:mm") == "06:15") {
                            stop = 1;
                            break;
                        }
                    }
                }
                catch(Exception e) {
                    Console.WriteLine("In Main, Error Occured: " + e.ToString());
                    using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                    combined = "In Main, Error Occured: " + e.ToString();
                        Log(combined, log_w);
                    }
                }
            }
            Console.WriteLine("Program Momentarily Shutting Down. Will Resume Shortly.");
            using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                Log("Program Momentarily Shutting Down. Will Resume Shortly.", log_w);
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
            using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + ".txt", true)) {
                Program.Log("One or more of the cells lost connection, re-establishing now.", log_w);
            }
        }

        private void recieve() {
            string combined = null;
            var csv0 = new StringBuilder();
            var csv9 = new StringBuilder();
            var csv13 = new StringBuilder();
            var csv14 = new StringBuilder();
            var csv23 = new StringBuilder();
            var csv24 = new StringBuilder();
            var csv33 = new StringBuilder();
            var csv34 = new StringBuilder();
            var csv43 = new StringBuilder();
            var csv44 = new StringBuilder();
            var csv99 = new StringBuilder();
            byte[] bytes = new byte[400];
            DateTime currentDateTime = new DateTime();
            string data = null;
            string data2 = null;
            DateTime changedCurrentDateTime = new DateTime();
            string path = null;
            string path99 = null;
            //string path2 = null;
            //string path992 = null;
            int bytesRec = 0;
            string[] labels = {"Datum,SK_Nr,SchussNr,Service,TS_Nr,PrgNr,Schusszeit [s],GW  Ist    [g],Poly 1 Soll [g/s],Poly 1 Ist [g/s],Poly 2 Soll [g/s],Poly 2 Ist [g/s],Iso Soll [g/s],Iso Ist [g/s],Druck-Poly 1    SK[PSI],Druck-Poly 1   Pumpe[PSI],Druck-Poly 2     SK[PSI],Druck-Poly 2   Pumpe[PSI],Druck-Iso    SP[PSI],Druck-Iso   Pumpe[PSI],TempBeh-Poly 1  [°F],TempBeh-Poly 2  [°F],TempBeh-Iso  [°F],TempSK-Poly 1 [°F],TempSK-Poly 2 [°F],TempSK-Iso [°F],MV    Poly[%],MV    Iso[%],Luft,Glas,Poly 1 / Poly 2,Glasmenge,PresseNr,ToolNr,Part_Type", "date,SH_No,ShotNo,service,PS_No,prgNo,Shot time  [s],shot weight[g],Set Poly 1 [g/s],Act Poly 1 [g/s],Set Poly 2 [g/s],Act Poly 2 [g/s],Set Iso [g/s],Act Iso [g/s],Pressure-Poly 1 SH[PSI],Pressure-Poly 1 pump[PSI],Pressure-Poly 2  SH[PSI],Pressure-Poly 2 pump[PSI],Pressure-Iso SH[PSI],pressure-Iso pump[PSI],TempTank-Poly 1 [°F],TempTank-Poly 2 [°F],TempTank-Iso [°F],TempSH-Poly 1 [°F],TempSH-Poly 2 [°F],TempSH-Iso [°F],Ratio-Poly[%],Ratio-Iso[%],Air,Glass,Poly 1 / Poly 2,Amount of glass,PressNo,ToolNo,Part_Type", "_____,_____,________,_______,_____,_____,______________,______________,_________________,________________,_________________,________________,______________,_____________,_______________________,_________________________,________________________,_________________________,____________________,______________________,____________________,____________________,_________________,__________________,__________________,_______________,_____________,____________,____,____,_______________,_______________,___________,__________,_______________"};
            string[] labels99 = {"Datum,Cell#Robot#,SK_Nr,SchussNr,Service,TS_Nr,PrgNr,Schusszeit [s],GW  Ist    [g],Poly 1 Soll [g/s],Poly 1 Ist [g/s],Poly 2 Soll [g/s],Poly 2 Ist [g/s],Iso Soll [g/s],Iso Ist [g/s],Druck-Poly 1    SK[PSI],Druck-Poly 1   Pumpe[PSI],Druck-Poly 2     SK[PSI],Druck-Poly 2   Pumpe[PSI],Druck-Iso    SP[PSI],Druck-Iso   Pumpe[PSI],TempBeh-Poly 1  [°F],TempBeh-Poly 2  [°F],TempBeh-Iso  [°F],TempSK-Poly 1 [°F],TempSK-Poly 2 [°F],TempSK-Iso [°F],MV    Poly[%],MV    Iso[%],Luft,Glas,Poly 1 / Poly 2,Glasmenge,PresseNr,ToolNr,Part_Type,Cream_Time,End_Of_Rise,Density,User_ID", "date,Cell#Robot#,SH_No,ShotNo,service,PS_No,prgNo,Shot time  [s],shot weight[g],Set Poly 1 [g/s],Act Poly 1 [g/s],Set Poly 2 [g/s],Act Poly 2 [g/s],Set Iso [g/s],Act Iso [g/s],Pressure-Poly 1 SH[PSI],Pressure-Poly 1 pump[PSI],Pressure-Poly 2  SH[PSI],Pressure-Poly 2 pump[PSI],Pressure-Iso SH[PSI],pressure-Iso pump[PSI],TempTank-Poly 1 [°F],TempTank-Poly 2 [°F],TempTank-Iso [°F],TempSH-Poly 1 [°F],TempSH-Poly 2 [°F],TempSH-Iso [°F],Ratio-Poly[%],Ratio-Iso[%],Air,Glass,Poly 1 / Poly 2,Amount of glass,PressNo,ToolNo,Part_Type,Cream_Time,End_Of_Rise,Density,User_ID", "_____,________________,_____,________,_______,_____,_____,______________,______________,_________________,________________,_________________,________________,______________,_____________,_______________________,_________________________,________________________,_________________________,____________________,______________________,____________________,____________________,_________________,__________________,__________________,_______________,_____________,____________,____,____,_______________,_______________,___________,___________,_______________,_______________,_______________,___________,__________"};
            for(int i = 0; i < 3; i++) {
                csv0.AppendLine(labels[i]);
            }
            for(int i = 0; i < 3; i++) {
                csv9.AppendLine(labels99[i]);
            }
            while(true) {
                try {
                    Array.Clear(bytes, 0, bytes.Length);
                    //using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + "_" + data2.Substring(1,2) + ".txt", true)) {
                    //    combined = "Waiting for data... @ " + Program.GetLocalIPAddress();
                    //    Program.Log(combined, log_w);
                    //}
                    NetworkStream networkStream = clientSocket.GetStream();
                    bytesRec = networkStream.Read(bytes, 0, bytes.Length);
                    currentDateTime = DateTime.Now;
                    data = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    data += ",";
                    data2 = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    //data2 = Encoding.ASCII.GetString(bytes);
                    data += data2.Substring(5);
                    //using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + "_" + data2.Substring(1,2) + ".txt", true)) {
                    //    combined = "Text received: " + data;
                    //    Program.Log2(combined, log_w);
                    //}
                    Console.WriteLine("Text received: {0}", data);
                    //Console.WriteLine("Text received: {0}", data2);
                    data = data.Substring(0, data.Length - 6);
                    changedCurrentDateTime = currentDateTime.AddHours(-6).AddMinutes(-30);
                    //path2 = "C:\\Test\\Shots_" + data2.Substring(1,4) + "_" + changedCurrentDateTime.ToString("yyyy.MM.dd") + "_" + changedCurrentDateTime.ToString("tt") + ".csv";
                    //path992 = "C:\\Test\\Shots_" + currentDateTime.ToString("yyyy.MM.dd") + ".csv";
                    //path = "O:\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\Cell" + data2.Substring(2,1) + "Test\\Robot" + data2.Substring(4,1) + "Test\\Shots_" + data2.Substring(1,4) + "_" + changedCurrentDateTime.ToString("yyyy.MM.dd") + "_" + changedCurrentDateTime.ToString("tt") + ".csv";
                    path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\Cell" + data2.Substring(2,1) + "Test\\Robot" + data2.Substring(4,1) + "Test\\Shots_" + data2.Substring(1,4) + "_" + changedCurrentDateTime.ToString("yyy.MM.dd") + "_" + changedCurrentDateTime.ToString("tt") + ".csv";
                    //path = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\Cell " + data2.Substring(2,1) + "\\Robot " + data2.Substring(4,1) + "\\Shots_" + data2.Substring(1,4) + "_" + changedCurrentDateTime.ToString("yyy.MM.dd") + "_" + changedCurrentDateTime.ToString("tt") + ".csv";
                    path99 = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\Program99Test\\Shots_" + currentDateTime.ToString("yyy.MM.dd") + ".csv";
                    //path99 = "\\\\susrocfs2.global.web-int.net\\DET-Data\\MANUFACTURING\\Dept_Shared\\PHC\\Curves and Histories Backups\\Program99\\Shots_" + currentDateTime.ToString("yyy.MM.dd") + ".csv";
                    if(data2.Substring(1,4) == "C9R9") {
                        if(!File.Exists(path99)) {
                            File.AppendAllText(path99, csv9.ToString());
                        }
                        csv99.AppendLine(data);
                        File.AppendAllText(path99, csv99.ToString());
                        csv99.Clear();
                    }
                    else if(!File.Exists(path)) {
                        ile.AppendAllText(path, csv0.ToString());
                    }
                    if(data2.Substring(1,4) == "C1R3") {
                        csv13.AppendLine(data);
                        File.AppendAllText(path, csv13.ToString());
                        csv13.Clear();
                    }
                    else if(data2.Substring(1,4) == "C1R4") {
                        csv14.AppendLine(data);
                        File.AppendAllText(path, csv14.ToString());
                        csv14.Clear();
                    }
                    else if(data2.Substring(1,4) == "C2R3") {
                        csv23.AppendLine(data);
                        File.AppendAllText(path, csv23.ToString());
                        csv23.Clear();
                    }
                    else if(data2.Substring(1,4) == "C2R4") {
                        csv24.AppendLine(data);
                        File.AppendAllText(path, csv24.ToString());
                        csv24.Clear();
                    }
                    else if(data2.Substring(1,4) == "C3R3") {
                        csv33.AppendLine(data);
                        File.AppendAllText(path, csv33.ToString());
                        csv33.Clear();
                    }
                    else if(data2.Substring(1,4) == "C3R4") {
                        csv34.AppendLine(data);
                        File.AppendAllText(path, csv34.ToString());
                        csv34.Clear();
                    }
                    else if(data2.Substring(1,4) == "C4R3") {
                        csv43.AppendLine(data);
                        File.AppendAllText(path, csv43.ToString());
                        csv43.Clear();
                    }
                    else if(data2.Substring(1,4) == "C4R4") {
                        csv44.AppendLine(data);
                        File.AppendAllText(path, csv44.ToString());
                        csv44.Clear();
                    }
                }
                catch(Exception e) {
                    Console.WriteLine("In Thread, Error Occured: " + e.ToString());
                    if(e.ToString().Substring(0, 73) == "System.IO.IOException: Unable to read data from the transport connection:") {
                        clientSocket.Close();
                        Program.RemoveClient(clientSocket);
                        return;
                    }
                    else if(e.ToString().Substring(0, 71) == "System.IO.DirectoryNotFoundException: Could not find a part of the path") {
                        using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + "_" + data2.Substring(1,2) + ".txt", true)) {
                            combined = "In Thread, Error Occured: " + e.ToString();
                            Program.Log(combined, log_w);
                        }
                        Console.WriteLine("Note: Cannot access O-Drive or the target folder within it.");
                    }
                    else if(e.ToString().Substring(0, 57) == "System.IO.IOException: The process cannot access the file") {
                        using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + "_" + data2.Substring(1,2) + ".txt", true)) {
                            combined = "In Thread, Error Occured: " + e.ToString();
                            Program.Log(combined, log_w);
                        }
                        Console.WriteLine("Note: Target CSV File In Use.");
                    }
                    else {
                        using(var log_w = new StreamWriter("C:\\Test\\Log\\Log_" + DateTime.Now.ToString("MM.dd.yyyy") + "_" + data2.Substring(1,2) + ".txt", true)) {
                            combined = "In Thread, Error Occured: " + e.ToString();
                            Program.Log(combined, log_w);
                            Program.Log2("Unexpected Error Detected. Program will pause for 10 seconds", log_w);
                        }
                        Console.WriteLine("Unexpected Error Detected. Program will pause for 10 seconds");
                        Thread.Sleep(10000);
                    }
                }
            }
        }
    }
}
