using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Net.Http;
using DeathByCaptcha;
using CsvHelper;
using System.Globalization;
using RestSharp;

namespace ShipStation
{
    class checkit
    {
        
        

        string checklogin;
        string password = Properties.Settings.Default.password;

        [Obsolete]
        public void onlyVidaPay(string user, string from, string apikey, string secretkey, string orderid)
        {
            Form1 frm = new Form1();
            frm.cancelation();
            var options = new ChromeOptions();
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            if (Properties.Settings.Default.visib == false)
            {
                options.AddArgument("--headless --disable-gpu");
            }
            //
            IWebDriver driver = new ChromeDriver(chromeDriverService, options);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            OpenQA.Selenium.Interactions.Actions action = new Actions(driver);



            //new vida pay


            bool stop = frm.msg();

            if (stop == true)
            {
                return;
            }

            driver.Navigate().GoToUrl("https://www.vidapay.com/home?challengeAuthority=false");

            driver.FindElement(By.Id("login-btn")).Click();

            Task.Delay(5000).Wait();
            driver.FindElement(By.Id("AccountId")).SendKeys(Properties.Settings.Default.vidaid);
            driver.FindElement(By.Id("Username")).SendKeys(Properties.Settings.Default.vidauser);
            driver.FindElement(By.Id("Password")).SendKeys(Properties.Settings.Default.vidapass);
            Task.Delay(5000).Wait();
            IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;

            jse.ExecuteScript("scroll(250, 0)"); // if the element is on top.

            jse.ExecuteScript("scroll(0, 250)"); // if the element is on bottom.
            driver.FindElement(By.XPath("//*[@id='btnClick']")).Click();

            string siteky;
            try
            {
                siteky = driver.FindElement(By.ClassName("g-recaptcha")).GetAttribute("data-sitekey");
            }
            catch
            {
                siteky = "";
            }


            if (siteky != "")
            {


                Solve_Captcha(driver,jse,siteky);
            }
            stop = frm.msg();

            if (stop == true)
            {
                return;
            }
            Task.Delay(7000).Wait();
            driver.FindElement(By.Id("18")).Click();
            Task.Delay(9000).Wait();

          
            // finding att--------------------------------------------------------------
            var divAtt = driver.FindElements(By.ClassName("express-category"))[0];
            var AttSpanSelect = divAtt.FindElement(By.ClassName("select_box"));//var cat=document.getElementsByClassName("express-category")[0]; var smart=cat.getElementsByClassName("select_box")[0];  debugger;
            AttSpanSelect.Click();
            Task.Delay(1000).Wait();
            var allAttLi = driver.FindElements(By.TagName("li"));//option 5
            for (int liat = 0; liat < allAttLi.Count; liat++)
            {
                if (allAttLi[liat].Text.IndexOf("AT&T RTR $10-$100") > -1)
                {
                    allAttLi[liat].Click();
                    break;
                }
            }
        
          
            Task.Delay(2000).Wait();
            //"ui-id-15"----36
            Task.Delay(3000).Wait();
            var divtextAtt = divAtt.FindElement(By.Id("ExpressCheckoutForm"));
            divtextAtt.FindElement(By.Id("Addon8")).SendKeys(user);
            Task.Delay(2000).Wait();
            string succes = "";
            divtextAtt.FindElement(By.Id("Addon4")).SendKeys(Properties.Settings.Default.price);
            Task.Delay(3000).Wait();
            try
            {
                succes = driver.FindElement(By.Id("balanceError")).GetAttribute("style");
            }
            catch { }
            if (succes.IndexOf("block")>-1)
            {
                WriteLogErr(user, from);
                driver.Quit();
                return;
            }

            divtextAtt.FindElement(By.Id("express-btn")).Click();
            Task.Delay(12000).Wait();
            succes = "";
            try
            {


                succes = driver.FindElements(By.TagName("table"))[1].FindElements(By.TagName("td"))[1].Text;
            }
            catch
            {
                succes = "";
            }
            if (succes == "")
            {
                WriteLogErr(user, from);
                driver.Quit();
                return;
            }

            if (succes.IndexOf("$0.00") > -1)
            {
                try
                {
                    driver.FindElement(By.Id("18")).Click();
                }
                catch
                {
                    driver.Navigate().GoToUrl("https://www.vidapay.com/topup/index?categoryid=18");
                }
                Task.Delay(9000).Wait();
                goto verPay_;
            }
            else
            {
                markAsShipped(apikey, secretkey, orderid);
                WriteLogSuccess(user, from);
               
                //---------------------------------------------------
                driver.Quit();
                return;
            }

            verPay_:
            // finding verizon and payment
            var divVerizoncat = driver.FindElements(By.ClassName("express-category"));
            IWebElement divVerizon = null;
            foreach (var div in divVerizoncat)
            {
                if (div.GetAttribute("data-name").ToLower() == "verizon wireless")
                {
                    divVerizon = div;
                    break;
                }
            }
         
            Task.Delay(1000).Wait();
           
            //"ui-id-15"----36
            Task.Delay(3000).Wait();
            var divtext = divVerizon.FindElement(By.Id("Addon8"));
            divtext.SendKeys(user);
            Task.Delay(2000).Wait();
            divVerizon.FindElement(By.Id("smartButton")).Click();

            Task.Delay(5000).Wait();
         

            var invalid = divVerizon.FindElements(By.TagName("strong"));
            for (int invalidver =0; invalidver< invalid.Count;invalidver++)
            {
                if (invalid[invalidver].Text.IndexOf("invalid number") > -1)
                {
                    WriteLogErr(user, from);
                    driver.Quit();
                    return;
                }
             
            }

            string balanceVerizon = divVerizon.FindElements(By.ClassName("row"))[1].FindElements(By.TagName("span"))[1].Text;
            if (balanceVerizon == "$0.00")
            {
                Task.Delay(3000).Wait();
                
                
                divVerizon.FindElement(By.Id("smartButton")).Click();
            }
            else
            {
                Task.Delay(3000).Wait();
                TextWriter tw47 = new StreamWriter("Log.txt", true);
                tw47.WriteLine("Has Balance: " + balanceVerizon+ "   "+ DateTime.Now.ToString() + " Phone:" + user + " from:Verizon " + from);
                tw47.Close();
                divVerizon.FindElement(By.Id("smartButton")).Click();
            }
            stop = frm.msg();

            if (stop == true)
            {
                return;
            }

            Task.Delay(12000).Wait();
            //----------------------------------------------------------------------------

            //table check
            succes = "";
            try
            {


                succes = driver.FindElements(By.TagName("table"))[1].FindElements(By.TagName("td"))[1].Text;
            }
            catch
            {
                succes = "";
            }
            if (succes == "" || succes.IndexOf("$0.00") > -1)
            {

                WriteLogErr(user, from);
                driver.Quit();
                return;
            }

           
            else
            {
                markAsShipped(apikey, secretkey, orderid);
                WriteLogSuccess(user, from);
                //---------------------------------------------------
                driver.Quit();
                return;
            }

        }

        private void WriteLogSuccess(string user, string from)
        {
            TextWriter tw31 = new StreamWriter("Log.txt", true);
            tw31.WriteLine("Successfull payed " + DateTime.Now.ToString() + " Phone:" + user + " from: " + from);
            tw31.Close();
        }

        private void WriteLogErr(string user, string from)
        {
            TextWriter tw41 = new StreamWriter("Log.txt", true);
            tw41.WriteLine("Error Not Found " + DateTime.Now.ToString() + " Phone:" + user + " from:Att " + from);
            tw41.Close();
            
        }

        private void Solve_Captcha(IWebDriver driver, IJavaScriptExecutor jse, string siteky)
        {
            JObject a = new JObject(

                   new JProperty("proxy", ""),
                   new JProperty("proxytype", ""),
                   new JProperty("googlekey", siteky),
                  new JProperty("pageurl", "https://id.vidapay.com/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Dvidapay%26redirect_uri%3Dhttps%253A%252F%252Fwww.vidapay.com%252Foidc%252Fcallback%26response_mode%3Dform_post%26response_type%3Dcode%2520id_token%26scope%3Dopenid%2520profile%26state%3DOpenIdConnect.AuthenticationProperties%253D2RU92WtcLW8HlbtZ9BWR0Upf8MtUG8l9i_iH99jeRNcUgUGR-CmdQMLORywIsGUd7mmcI3kM4Miciglo0rwvsChoTs5jIF9ciZwryadOJ5eSUUYPVpUH3P3pAm2hK0IIiahLhfDQL6m8AQfKvEo0hPpdKSNBho3llU1EI1QrazzZpeUmrJxj7a0HuMBQ0Wx1NtwrL8PunvK9oaUKJHKKqlZXm5QKdLkJK7iOuQiopFw%26nonce%3D637434816620765291.ZjRmYzRjMjMtNDNmNy00NDhhLWE3ZTAtZjc5YTUwZmU0ZGI5ZGIyZmUyZGUtZDE1MC00NjE1LWFiMjMtMmFhNDVjYjViNGI4%26x-client-SKU%3DID_NET461%26x-client-ver%3D5.3.0.0"));

            StreamWriter sw = new StreamWriter("capt.json");
            sw.Write(a);
            sw.Close();

            Task.Delay(500).Wait();
            string pageurl = "https://id.vidapay.com/Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Dvidapay%26redirect_uri%3Dhttps%253A%252F%252Fwww.vidapay.com%252Foidc%252Fcallback%26response_mode%3Dform_post%26response_type%3Dcode%2520id_token%26scope%3Dopenid%2520profile%26state%3DOpenIdConnect.AuthenticationProperties%253D2RU92WtcLW8HlbtZ9BWR0Upf8MtUG8l9i_iH99jeRNcUgUGR-CmdQMLORywIsGUd7mmcI3kM4Miciglo0rwvsChoTs5jIF9ciZwryadOJ5eSUUYPVpUH3P3pAm2hK0IIiahLhfDQL6m8AQfKvEo0hPpdKSNBho3llU1EI1QrazzZpeUmrJxj7a0HuMBQ0Wx1NtwrL8PunvK9oaUKJHKKqlZXm5QKdLkJK7iOuQiopFw%26nonce%3D637434816620765291.ZjRmYzRjMjMtNDNmNy00NDhhLWE3ZTAtZjc5YTUwZmU0ZGI5ZGIyZmUyZGUtZDE1MC00NjE1LWFiMjMtMmFhNDVjYjViNGI4%26x-client-SKU%3DID_NET461%26x-client-ver%3D5.3.0.0";
            string proxy = "http://balpi:HavBer+2121@127.0.0.1:1234";
            string proxyType = "HTTP";
            Client client = (Client)new DeathByCaptcha.HttpClient(Properties.Settings.Default.dbcuser, Properties.Settings.Default.dbcpass);


            double balance = client.GetBalance();
            Console.WriteLine(balance);


            string tokenParams = "{\"proxy\": \"" + proxy + "\"," +
            "\"proxytype\": \"" + proxyType + "\"," +
            "\"googlekey\": \"" + siteky + "\"," +
            "\"pageurl\": \"" + pageurl + "\"}";

            for (int i = 0; i < 3; i++)
            {


                Captcha captcha = client.Decode("capt.json", 120,
                    new Hashtable() { { "type", 4 }, { "token_params", tokenParams } });

                if (null != captcha)
                {
                    Console.WriteLine("CAPTCHA {0}: {1}", captcha.Id, captcha.Text);
                    jse.ExecuteScript("document.getElementById('g-recaptcha-response').innerHTML ='" + captcha.Text + "';");

                    break;
                }
            }
            jse.ExecuteScript("scroll(250, 0)");

            jse.ExecuteScript("scroll(0, 250)");

            jse.ExecuteScript("document.getElementById('btnClick').removeAttribute('disabled')");
            try
            {


                jse.ExecuteScript("document.getElementById('btnClick').Click()");
            }
            catch
            {

                driver.FindElement(By.XPath("//*[@id='btnClick']")).Click();
            }
        }

      
        
        [Obsolete]
        public void hotSpot(string user, string passwor, string invoice,string apikey,string secretkey,string orderid)
        {
            Form1 frm = new Form1();
            bool stop = frm.msg();

            if (stop == true)
            {
                return;
            }
            var options = new ChromeOptions();
            if (Properties.Settings.Default.visib == false)
            {
                options.AddArgument("--headless --disable-gpu");
            }
            var chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            IWebDriver driver = new ChromeDriver(chromeDriverService, options);
            OpenQA.Selenium.Interactions.Actions action = new Actions(driver);

            string getUrl = "https://hotspotunlimiteddata.us/admin/authentication";
            driver.Navigate().GoToUrl(getUrl);
            driver.FindElement(By.Id("email")).SendKeys(user);
            driver.FindElement(By.Id("password")).SendKeys(passwor);
            driver.FindElement(By.XPath("//*[@class='form-group']/button")).Click();

            Task.Delay(5000).Wait();
            action.SendKeys(OpenQA.Selenium.Keys.Escape);
            stop = frm.msg();

            if (stop == true)
            {
                return;
            }
            try
            {
                checklogin = driver.FindElement(By.Id("search_input")).GetAttribute("placeholder").ToString();
            }
            catch
            {
                checklogin = "";
            }
            //if login

            if (checklogin != "")
            {
                driver.FindElement(By.Id("search_input")).SendKeys(invoice);
                Task.Delay(5000).Wait();
               //var a = driver.FindElements(By.XPath("///ul[@id='top_search_dropdown']//a"));
                driver.FindElement(By.XPath("//a[contains(text(),'"+invoice+"')]")).Click();
                
                Task.Delay(2000).Wait();
                string pnumber = driver.FindElement(By.XPath("//*//td[@class='description']//span[1]")).Text;
                pnumber = pnumber.Substring(pnumber.Length - 10, 10);

                TextWriter tw = new StreamWriter("hotspot_log.txt", true);
                tw.WriteLine("SUCCESSFUL  get the PHONE NUMBER: " + DateTime.Now.ToString() + " Invoice: " + invoice + " Phone Number: " + pnumber);
                tw.Close();

                TextWriter tw2 = new StreamWriter("Log.txt", true);
                tw2.WriteLine("SUCCESSFUL get the PHONE NUMBER: " + DateTime.Now.ToString() + " Invoice: " + invoice + " Phone Number: " + pnumber);
                tw2.Close();

                driver.Quit();
                
                if (pnumber.Replace("-","").Trim().Length < 10)
                {
                    return;
                }
                bool exs = read_boom(pnumber);
                if (exs == true)
                {
                    TextWriter tw123 = new StreamWriter("Log.txt", true);
                    tw123.WriteLine("BOOM RECORD: " + DateTime.Now.ToString() + " Invoice: " + invoice + " Phone Number: " + pnumber);
                    tw123.Close();
                    return;
                }
                else
                {
                   
                    stop = frm.msg();

                    if (stop == true)
                    {
                        return;
                    }
                    onlyVidaPay(pnumber, "HotSpot",apikey,secretkey,orderid);
                }

            }
            else
            {
                TextWriter tw = new StreamWriter("hotspot_log.txt", true);
                tw.WriteLine("ERROR----- occured " + DateTime.Now.ToString() + " Invoice: " + invoice + " Phone Number is not found ");
                tw.Close();
                TextWriter tw2 = new StreamWriter("Error.txt", true);
                tw2.WriteLine("ERROR----- occured " + DateTime.Now.ToString() + " Invoice: " + invoice + " Phone Number is not found ");
                tw2.Close();
            }

        }
        public bool read_boom(string pnumber)
        {
            bool exs = false;
            var reader = new StreamReader("Boom Accounts.csv");
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords < boom>();

            foreach (var r in records)
            {
                if (r.MDN == pnumber)
                {
                    exs = true;
                    break;
                }
            }
            return exs;
        }

        public void markAsShipped(string apikey, string apisecret, string orderid)
        {
            try
            {


                var client = new RestClient("https://ssapi.shipstation.com/orders/markasshipped/");
                client.Timeout = -1;
                client.Authenticator = new HttpBasicAuthenticator(apikey, apisecret);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Host", "ssapi.shipstation.com");
                request.AddParameter("application/json", "{\n  \"orderId\":" + orderid + ",\n  \"carrierCode\": \"usps\"\n}", ParameterType.RequestBody);
                //request.AddHeader("Authorization", authkey8);
                request.AddHeader("Host", "ssapi.shipstation.com");
                IRestResponse response = client.Execute(request);
                JObject o = JObject.Parse(response.Content);
                try
                {
                    TextWriter tw = new StreamWriter("Log.txt", true);
                    tw.WriteLine("DELETED: " + orderid);
                    tw.Close();
                }
                catch { }
            }
            catch { }
        }



    }
}
