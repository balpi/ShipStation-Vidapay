using System;
using System.Diagnostics;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.IO;
using System.ComponentModel;

namespace ShipStation
{
    public partial class Form1 : Form
    {
        public string AuthorizationString;
        string uname,varstring;
       public string path = Application.StartupPath +"/";
        
        public Form1()
        {
            InitializeComponent();
            
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            Properties.Settings.Default.api = txtapi.Text;
            Properties.Settings.Default.secret = txtsecret.Text;
            Properties.Settings.Default.hotspotuser = textBox1.Text;
            Properties.Settings.Default.hotspotpass = textBox2.Text;
            Properties.Settings.Default.password = textBox3.Text;
            Properties.Settings.Default.vidaid=txtid.Text;
            Properties.Settings.Default.vidauser=txtuser.Text;
            Properties.Settings.Default.vidapass=txtpass.Text;
            Properties.Settings.Default.dbcuser=txtdbcuser.Text;
            Properties.Settings.Default.dbcpass=txtdbcpass.Text;
            Properties.Settings.Default.price=price.Text;

            if (checkBox1.Checked == true)
            {
                Properties.Settings.Default.visib = true;
            }
            else
            {
                Properties.Settings.Default.visib = false;
            }

            Properties.Settings.Default.Save();


            //checkit ck = new checkit();
            //ck.onlyVidaPay("4046933002", "verizon", "s", "s", "s");


            btn_start.Enabled = false;
            button1.Enabled = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.RunWorkerAsync();
            System.Windows.Forms.Timer mytimer = new System.Windows.Forms.Timer();
            mytimer.Interval=30 * 60 * 1000;
            mytimer.Enabled = true;
            mytimer.Tick += new System.EventHandler(OnTimerEvent);


            mytimer.Enabled = true;

            
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                backgroundWorker1.RunWorkerAsync();
            });
        }

        
        //changed shipped
       
        private void btn_err_Click(object sender, EventArgs e)
        {
            var a = File.Exists("CardError.txt");
            if (a == true)
            {
                Process.Start("CardError.txt");
            }
            else
            {
                MessageBox.Show("The File Not Found");
            }

            
        }

        private void btn_log_Click(object sender, EventArgs e)
        {
            var a = File.Exists("Log.txt");
            if (a == true)
            {
                Process.Start("Log.txt");
            }
            else
            {
                MessageBox.Show("The File Not Found");
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                backgroundWorker1.CancelAsync();
                var b = backgroundWorker1.CancellationPending;
                    button1.Enabled = false;
                    btn_start.Enabled = true;
                cancelation();
            }
            catch { }
        }
        public bool msg()
        {
            bool stop=false;
            if (button1.Enabled == false)
            {
                stop = true;
            }
            return stop;
        }
        public void cancelation()
        {
            if (backgroundWorker1.CancellationPending == true)
            {

                backgroundWorker1.CancelAsync();
              
            }
        }
        JObject GetOrders()
        {
            string apikey = txtapi.Text;
            string secretkey = txtsecret.Text;
            var authkey2 = System.Text.Encoding.UTF8.GetBytes(apikey + ":" + secretkey);
            string authkey8 = System.Convert.ToBase64String(authkey2);
            authkey8 = "Basic " + authkey8.Substring(0, 70);
            var client = new RestClient("https://ssapi.shipstation.com/");
            client.Timeout = -1;
            client.Authenticator = new HttpBasicAuthenticator(apikey, secretkey);
            var request = new RestRequest("/orders/listbytag?orderStatus=awaiting_shipment", Method.GET);
            request.AddHeader("Host", "ssapi.shipstation.com");
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = client.Execute(request);
            return JObject.Parse(response.Content);
        }
        [Obsolete]
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                
                BackgroundWorker helperBW = sender as BackgroundWorker;
               
                DateTimeOffset lastproc;
                var o = GetOrders();
                var roles = o.Value<JArray>("orders");
                var ocount = Convert.ToDecimal(roles.Count);

                for (int i = 0; i < ocount; i++)
                {
                    if (backgroundWorker1.CancellationPending == true)
                    {
                        e.Cancel=true;
                        backgroundWorker1.Dispose();
                        return;
                    }

                    if (button1.Enabled == false)
                    {
                        backgroundWorker1.CancelAsync();
                        backgroundWorker1.Dispose();

                        return; 
                    }
                   
                        lastproc = DateTime.Now.AddYears(-1);


                    DateTimeOffset CreateDate = (DateTimeOffset)o.SelectToken("orders.[" + i + "].items.[0].createDate");
                    uname = (string)o.SelectToken("orders.[" + i + "].items.[0].name");
                    if (lastproc < CreateDate)
                    {
                        if ( i == ocount - 1)
                        {
                            TextWriter tw = new StreamWriter("Last.txt");
                            tw.WriteLine((DateTimeOffset)o.SelectToken("orders.[0].items.[0].createDate"));
                            tw.Close();
                        }
                      
                        if (uname == "" || uname.Substring(0,3)=="Ver" || uname.Substring(0, 3) == "ATT" || uname.Substring(0,3)=="SIM")
                        {
                            continue;
                        }

                        if (uname.Substring(0, 3) == "Pay")
                        {
                            varstring = uname.Substring(20, uname.Length - 20);
                            checkit ck = new checkit();
                            ck.hotSpot(textBox1.Text, textBox2.Text, varstring,txtapi.Text,txtsecret.Text,
                                o.SelectToken("orders.[" + i + "].orderId").ToString());
                            continue;
                            
                        }
                        if (uname.Substring(0, 3) == "Unl")
                        {
                            if (uname.IndexOf("LTE") > -1)
                            {
                                varstring = uname.Substring(22, uname.Length - 22);
                            }
                            else
                            {
                                varstring = uname.Substring(18, uname.Length - 18);
                            }
                            
                            if (varstring.Length > 10)
                            {
                                varstring = varstring.Substring(varstring.Length - 10, 10);
                            }

                            if (varstring != varstring.Replace("-", ""))
                            {
                                
                                continue;
                            }

                            checkit ck = new checkit();
                            bool exs = ck.read_boom(varstring);
                            if (exs == true)
                            {
                                TextWriter tw123 = new StreamWriter("Log", true);
                                tw123.WriteLine("BOOM RECORD: " + DateTime.Now.ToString() + " Invoice:  Phone Number: " + varstring);
                                tw123.Close();
                                continue;
                            }
                            if (button1.Enabled == false)
                            {
                                backgroundWorker1.CancelAsync();
                                backgroundWorker1.Dispose();

                                return;
                            }
                            else
                            {
                                ck.onlyVidaPay(varstring, "ShipStation", txtapi.Text, txtsecret.Text,
                                    o.SelectToken("orders.[" + i + "].orderId").ToString());
                            }

                           
                        }
                       

                            int tring;
                        bool pat = false;
                        try
                        {
                            tring = Convert.ToInt32(uname);
                            pat = true;
                        }
                        catch
                        {
                            pat = false;
                        }
                        if (pat)
                        {
                            varstring = uname;
                            varstring = varstring.Replace("-", "");
                            checkit ck = new checkit();
                            bool exs = ck.read_boom(varstring);
                            if (exs == true)
                            {
                                TextWriter tw123 = new StreamWriter("Log", true);
                                tw123.WriteLine("BOOM RECORD: " + DateTime.Now.ToString() + " Invoice:  Phone Number: " + varstring);
                                tw123.Close();
                                continue;
                            }
                            else
                            {
                                ck.onlyVidaPay(varstring, "ShipStation", txtapi.Text, txtsecret.Text,
                                    o.SelectToken("orders.[" + i + "].orderId").ToString());
                            }

                            
                        }
                    }
                    if (helperBW.CancellationPending == true)
                    {
                        e.Cancel = true;
                    }
                    if (button1.Enabled == false)
                    {
                        backgroundWorker1.CancelAsync();
                        backgroundWorker1.Dispose();

                        return;
                    }

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker1.CancelAsync();
            if (e.Cancelled) MessageBox.Show("Stopped");
           
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (backgroundWorker1.CancellationPending == true)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            txtapi.Text = Properties.Settings.Default.api;
            txtsecret.Text=Properties.Settings.Default.secret ;
            textBox1.Text = Properties.Settings.Default.hotspotuser;
            textBox2.Text= Properties.Settings.Default.hotspotpass;
            textBox3.Text=Properties.Settings.Default.password ;
            txtid.Text = Properties.Settings.Default.vidaid;
            txtuser.Text = Properties.Settings.Default.vidauser;
            txtpass.Text = Properties.Settings.Default.vidapass;
            txtdbcuser.Text = Properties.Settings.Default.dbcuser;
            txtdbcpass.Text = Properties.Settings.Default.dbcpass;
            price.Text = Properties.Settings.Default.price;
            checkBox1.Checked = Properties.Settings.Default.visib;
            this.backgroundWorker1.WorkerReportsProgress = true;
        }
      

    }


}
