using Bussiness;
using Bussiness.Managers;
using System;
using System.Net;
using System.Configuration;

namespace Game.Server
{
    public class Update_TOP
    {
        private bool isuptop = false;
        private DateTime lastUpdate = DateTime.MinValue;
        private string req = ConfigurationManager.AppSettings["request"] + "CelebList/CreateAllCeleb.ashx";

        private void uptop()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadString(req);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("TU DONG UPDATE PHONG CAO THU THANH CONG!");
                Console.ResetColor();
            }
            catch
            {
                Console.WriteLine("ERROR");
            }
        }

        public void UpdateCeleb()
        {
            Console.WriteLine("CHECK UPDATE PHONG CAO THU, Last: " + lastUpdate);
            //if (DateTime.Now.Minute > 10)
            //    isuptop = false;
            if (DateTime.Now > lastUpdate.AddMinutes(60))
            {
                //isuptop = true;
                lastUpdate = DateTime.Now;
                this.uptop();
            }

        }
    }
}