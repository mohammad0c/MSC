﻿using System;
using System.Net;
using MSC.Brute;
using System.Drawing;
using System.Text;
using System.IO;

namespace MSC
{
    class Utils
    {
        public static long HashCode(string input)
        {
            int x, y, R;
            string neh = "";
            x = int.Parse(input);
            y = (x * x) / (2 * x);
            R = ((y * y) / (2 * y)) * x;
            y = (R + x) * (R + x);
            neh = R.ToString() + y.ToString();

            return long.Parse(neh);
        }
        public static RequestManage GetManage(WebResponse Wr,CookieContainer Cc)
        {
            RequestManage NewManage = new RequestManage();
            NewManage.Cookies = Cc;
            NewManage.Headers = Wr.Headers;
            NewManage.SourcePage = new System.IO.StreamReader(Wr.GetResponseStream()).ReadToEnd();
            NewManage.Location = Wr.Headers["Location"];
            NewManage.RedirectedUrl = Wr.ResponseUri.AbsoluteUri;
            //if (Wr.Headers["Content-Type"].Contains("image/"))
            //{
            //    Stream httpResponseStream = Wr.GetResponseStream();

            //    NewManage.Image = Image.FromStream(httpResponseStream);
            //}

            return NewManage;
        }
        public static string GetCookiesString(CookieContainer cookies, Config config)
        {
            try {
                Uri ur = new Uri(config.LoginURL);
                return cookies.GetCookieHeader(ur);
            }
            catch(Exception ex) { return "Error to Get Cookies: " + ex.Message; }
        }

        public static WebHeaderCollection SetHeaders(Config config)
        {
            WebHeaderCollection httpwebrequest = new WebHeaderCollection();
            string[] All = config.Headers.Split('|');
            foreach (string Item in All)
            {
                if (Item != "")
                    httpwebrequest.Add(Item);
            }
            return httpwebrequest;
        }

        public static CookieContainer SetCookies(Config config, CookieContainer cookieContainer, string Host)
        {
            while (config.Cookies.Contains(" "))
                config.Cookies = config.Cookies.Replace(" ", "");
            string[] strArray = config.Cookies.Split(';');
            foreach (string item in strArray)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    string back, forward;
                    string[] strArray1 = item.Split('=');
                    back = strArray1[0];
                    forward = strArray1[1];
                    cookieContainer.Add(new Cookie(back, forward) { Domain = Host });
                }
            }
            return cookieContainer;
        }
    }
}
