using System;

namespace GSMSMS
{
    class Program
    {
        static void Main(string[] args)
        {
            GSMsms sms = new GSMsms();

            //sms.RechercherModem();
            //les deux lignes suivantes permettent de chercher l'appareil GSM et d'afficher le message correspondant au résultat
            sms.Connecter();
            Console.WriteLine(sms.IsConnected);
            
            //sms.Deconnecter();
            //Console.WriteLine(sms.IsConnected);

            if (sms.IsConnected)
            {
               //sms.LireSms();
               sms.EnvoyerSms("695585034", "Bonjour, voici mon premier test en C#");
            }
          
            Console.Read();
        }
    }
}
