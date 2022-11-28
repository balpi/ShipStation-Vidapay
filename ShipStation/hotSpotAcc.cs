using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Windows.Forms;

namespace ShipStation
{
    class hotSpotAcc
    {
     
        public void hotSpot()
        {
           





            /*WebRequest getRequest = WebRequest.Create(getUrl);
            getRequest.Proxy = null;
            getRequest.Method = "POST";
            getRequest.ContentType = "application/x-www-form-urlencoded";

            getRequest.Headers.Add("Cookie", cookieHeader);
            WebResponse getResponse = getRequest.GetResponse();
            using (StreamReader sr = new StreamReader(getResponse.GetResponseStream()))
            {
                pageSource = sr.ReadToEnd();
            }
            string newloginUri = "http://www.conquerclub.com/game.php?game=13025037";
            HttpWebRequest newrequest = (HttpWebRequest)WebRequest.Create(newloginUri);
            newrequest.Proxy = null;*/



        }
        private void b_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser b = sender as WebBrowser;

        }
    }
    }
