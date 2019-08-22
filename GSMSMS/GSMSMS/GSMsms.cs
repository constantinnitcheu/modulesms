using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace GSMSMS
{
    class GSMsms
    {
        private SerialPort gsmPort = null;
        private bool IsDeviceFound { get; set; } = false;
        public bool IsConnected { get; set; } = false;

        public GSMsms()
        {
            gsmPort = new SerialPort();
        }

        public GSMcom[] List()
        {
            List<GSMcom> gsmcom = new List<GSMcom>();
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.EnablePrivileges = true;

            try
            {
                string path = $@"\\{Environment.MachineName}\root\cimv2";
                ManagementScope scope = new ManagementScope(path, options);
                scope.Connect();

                ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_POTSModem");
                ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query);
                ManagementObjectCollection collection = search.Get();

                foreach (ManagementObject obj in collection)
                {
                    string portName = obj["AttachedTo"].ToString();
                    string portDescription = obj["Description"].ToString();

                    if (portName != "")
                    {
                        GSMcom com = new GSMcom();
                        com.Name = portName;
                        com.Description = portDescription;
                        gsmcom.Add(com);
                    }
                }

            } catch (StackOverflowException ex)
            {
                Console.WriteLine(ex.Message);
                //IsConnected = false;
            }        

            return gsmcom.ToArray();
        }
        // cette fonction permet de rechercher si un appareil GSM est connecté
        public GSMcom RechercherModem()
        {
            IEnumerator enumerator = List().GetEnumerator();
            GSMcom com = enumerator.MoveNext() ? (GSMcom)enumerator.Current : null;

            if (com == null)
            {
                IsDeviceFound = false;
                Console.WriteLine("Désolé aucun modem GSM trouvé!");
                Console.WriteLine("Veuillez brancher un appareil GSM et réessayer");
            }
            else
            {
                //affiche la description et le nom du modem puis le connecte au port
                IsDeviceFound = true;
                Console.WriteLine(com.ToString());
                Connecter();
            }
            return com;
        }

        //méthode Connect() permet de connecter l'appareil GSM au port
        public bool Connecter()
        {
            if (gsmPort == null || !IsConnected || !gsmPort.IsOpen)
            {
                GSMcom com = RechercherModem();
                if(com != null)
                {
                    try
                    {
                        gsmPort.PortName = com.Name; //COM2
                        gsmPort.BaudRate = 33600; //28800 192.168.10.10   192.168.1.117
                        gsmPort.Parity = Parity.None;
                        gsmPort.DataBits = 8;
                        gsmPort.StopBits = StopBits.One;
                        gsmPort.Handshake = Handshake.RequestToSend;
                        gsmPort.DtrEnable = true;
                        gsmPort.RtsEnable = true;
                        gsmPort.NewLine = Environment.NewLine;
                        gsmPort.Open();
                        IsConnected = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        IsConnected = false;
                    }
                } else
                {
                    IsConnected = false;
                }
            }
            return IsConnected;
        }

        //méthode Deconnecter() permet de déconnecter l'appareil GSM au port
        public void Deconnecter()
        {
            if (gsmPort != null || IsConnected || gsmPort.IsOpen)
            {
                gsmPort.Close();
                gsmPort.Dispose(); 
                IsConnected = false;
            }
           
        }

        public void LireSms()
        {
            Console.WriteLine("Lecture en cours...");

            gsmPort.WriteLine("AT+CMGF = 1"); //défini le mode du texte
            Thread.Sleep(1000); //donne le temps de lecture
            gsmPort.WriteLine("AT+CPMS = \"SM\""); // défini le stokage sur la SIM
            Thread.Sleep(1000);
            gsmPort.WriteLine("AT+CMGL = \"ALL\""); //defini la catégorie de lecture, REC READ ou REC UNREAD 
            Thread.Sleep(1000);

            string reponse = gsmPort.ReadExisting();

            if (reponse.EndsWith("\r\n0k\r\n"))
            {
                Console.WriteLine(reponse);
            }
            else
            {
                Console.WriteLine(reponse);
            }
        }

        public void EnvoyerSms(string toAdress, string message)
        {
            Console.WriteLine("Envoi en cours...");

            gsmPort.WriteLine("AT+CMGF = 1"); //défini le mode du texte
            Thread.Sleep(1000);
            gsmPort.WriteLine($"AT+CMGS =\"{toAdress}\'");
            Thread.Sleep(1000);
            gsmPort.WriteLine(message + char.ConvertFromUtf32(26));
            Thread.Sleep(5000);

            string reponse = gsmPort.ReadExisting();

            if (reponse.EndsWith("\r\nEnvoyé!!!\r\n") && reponse.Contains("+CMGS:"))
            {
                Console.WriteLine(reponse);
            } else
            {
                Console.WriteLine(reponse);
            }
        }
    }
}