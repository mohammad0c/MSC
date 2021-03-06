using System;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Security;
using System.Drawing;

namespace MSC.Brute
{
    class Requests
    {
        Logger logger = new Logger();
        public void SetLogger(Logger logg)
        {
            logger = logg;
        }
        public Logger GetLogger()
        {
            return logger;
        }

        private HttpWebRequest SetOtherSetting(Config config, HttpWebRequest httpWebRequest)
        {
            try
            {
                httpWebRequest.AllowAutoRedirect = config.AllowAutoRedirect;
                httpWebRequest.MaximumAutomaticRedirections = config.MaxRedirects;
            }
            catch { }
            if (config.UserAgent != null)
                httpWebRequest.UserAgent = config.UserAgent;

            httpWebRequest.KeepAlive = config.KeepAlive;

            if (config.Referer != null)
                httpWebRequest.Referer = config.Referer;

            if (config.ContectType != null)
                httpWebRequest.ContentType = config.ContectType;

            httpWebRequest.Method = config.Method.ToString();

            return httpWebRequest;
        }

        private string SetProxy(Proxy proxy, HttpWebRequest httpWebRequest, out HttpWebRequest res)
        {
            if (ProxyService.UseInAllRequest)
                proxy = ProxyService.proxy;
            if (proxy == null)
                if (ProxyService.proxy.Ip != null)
                    proxy = ProxyService.proxy;
            res = httpWebRequest;
            if (proxy.Ip == null)
                return "Proxy ip is null";
            if (proxy.Port == 0)
                return "Proxy  port is null";
            try
            {
                res.Proxy = new WebProxy(proxy.Ip, proxy.Port);
                if (proxy.Username != null && proxy.Password != null)
                {
                    NetworkCredential net = new NetworkCredential(proxy.Username, proxy.Password);
                    res.Proxy.Credentials = net;
                }
                return "OK";
            }
            catch (Exception ex) { logger.AddMessage("ERROR: can't Set proxy service : " + ex.Message, Log.Type.Error); return ex.Message; }
        }
        private string SetProxy(Proxy proxy, WebClient web, out WebClient res)
        {
            if (ProxyService.UseInAllRequest)
                proxy = ProxyService.proxy;
            if (proxy == null)
                if (ProxyService.proxy.Ip != null)
                proxy = ProxyService.proxy;
            res = web;
            if (proxy.Ip == null)
                return "Proxy ip is null";
            if (proxy.Port == 0)
                return "Proxy  port is null";
            try
            {
                res.Proxy = new WebProxy(proxy.Ip, proxy.Port);
                if (proxy.Username != null && proxy.Password != null)
                {
                    NetworkCredential net = new NetworkCredential(proxy.Username, proxy.Password);
                    res.Proxy.Credentials = net;
                }
                return "OK";
            }
            catch (Exception ex) { logger.AddMessage("ERROR: can't Set proxy service : " + ex.Message, Log.Type.Error); return ex.Message; }
        }


        internal RequestManage GetBytesRequest(Config config, Proxy proxy = null, bool GetImage = false)
        {
            WebClient wc = new WebClient();
            RequestManage Rm = new RequestManage();

            if (ProxyService.proxy.Ip != null | proxy != null)
            {
                string setProxy = SetProxy(proxy, wc, out wc);
                if (setProxy != "OK")
                {
                    Rm.Cookies = null;
                    Rm.Headers = null;
                    Rm.SourcePage = "ERROR|PROXY|" + setProxy;
                    logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + Rm.SourcePage + "\n\nCookies:\n" + Utils.GetCookiesString(Rm.Cookies, config) + "\n\nHeaders:\n" + Rm.Headers.ToString(), Log.Type.OutPut);
                    return Rm;
                }
            }
            else wc.Proxy = null;

            wc.Headers["Cookies"] = config.Cookies;
            wc.Headers["UserAgent"] = config.UserAgent;
            wc.Headers["KeepAlive"] = config.KeepAlive.ToString();
            wc.Headers["ContentType"] = config.ContectType;
            wc.Headers["Referer"] = config.Referer;

            byte[] Res = wc.DownloadData(config.LoginURL);
            if (GetImage)
            {
                try
                {
                    Image x = (Bitmap)((new ImageConverter()).ConvertFrom(Res));
                    Rm.Image = x;
                }
                catch { }
            }

            Rm.Bytes = Res;
            Rm.CookiesString = wc.Headers["Cookies"];
            Rm.Headers = wc.Headers;

            return Rm;
        }

        /// <summary>
		/// GETData base request
		/// </summary>
        /// <param name="config">Config for Request.</param>
        ///  <param name="mange">Keep your request by cookies.</param>
        ///  <param name="proxy">Send request by proxy service.</param>
        internal RequestManage GetPageSource(Config config, RequestManage mange = null, Proxy Proxy = null)
        {
            if (mange != null)
                if (mange.CookiesString != null)
                    config.Cookies += mange.CookiesString;
            RequestManage end = new RequestManage();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback +=
        new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
                CookieContainer container = new CookieContainer();

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(config.LoginURL);
                try
                {
                    if (mange.Cookies.Count != 0)
                    {
                        container = mange.Cookies;
                    }
                }
                catch { }

                //Set Proxy
                if (ProxyService.proxy.Ip != null | Proxy != null)
                {
                    string setProxy = SetProxy(Proxy, httpWebRequest, out httpWebRequest);
                    if (setProxy != "OK")
                    {
                        end.Cookies = null;
                        end.Headers = null;
                        end.SourcePage = "ERROR|PROXY|" + setProxy;
                        logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + end.SourcePage + "\n\nCookies:\n" + Utils.GetCookiesString(end.Cookies, config) + "\n\nHeaders:\n" + end.Headers.ToString(), Log.Type.OutPut);

                        return end;
                    }
                }
                else httpWebRequest.Proxy = null;

                httpWebRequest = SetOtherSetting(config, httpWebRequest);

                //Add Headres
                if (config.Headers != null)
                {
                    httpWebRequest.Headers = Utils.SetHeaders(config);
                }

                //Add Cookies
                if (config.Cookies != null)
                {
                    container = Utils.SetCookies(config, container, httpWebRequest.Host);
                }

                httpWebRequest.CookieContainer = container;


                logger.AddMessage("Getting Response", Log.Type.Infomation);

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();


                end = Utils.GetManage(response, container);
                end.CookiesString = Utils.GetCookiesString(container, config);

                logger.AddMessage("RequestManage OutPut\nCookies:\n" + Utils.GetCookiesString(end.Cookies, config) + "\n\nHeaders:\n" + end.Headers.ToString() + "\nCode: " + end.StatusCode.ToString(), Log.Type.OutPut);

                return end;

            }
            catch (WebException ex)
            {
                string error = ex.Message;

                end.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;

                end.ErrorAst = true;
                end.SourcePage = "ERROR|" + error + "|" + ex.Message;
                logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + end.SourcePage + "\n\nCode: " + end.StatusCode.ToString(), Log.Type.OutPut);

                return end;
            }
        }

        /// <summary>
		/// POSTData base request
		/// </summary>
        /// <param name="config">Config for Request.</param>
        ///  <param name="mange">Keep your request by cookies.</param>
        ///  <param name="proxy">Send request by proxy service.</param>
        internal RequestManage GetRequestByData(Config config, RequestManage mange = null, Proxy Proxy = null)
        {
            if (mange != null)
                if (mange.CookiesString != null)
                    config.Cookies += mange.CookiesString;
            RequestManage end = new RequestManage();
            try
            {
                //SetConfig
                CookieContainer container = new CookieContainer();
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(config.LoginURL);

                if (ProxyService.proxy.Ip != null | Proxy != null)
                {
                    string setProxy = SetProxy(Proxy, httpWebRequest, out httpWebRequest);
                    if (setProxy != "OK")
                    {
                        end.Cookies = null;
                        end.ErrorAst = true;
                        end.Headers = null;
                        end.SourcePage = "ERROR|PROXY|" + setProxy;
                        logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + end.SourcePage + "\n\nCookies:\n" + Utils.GetCookiesString(end.Cookies, config) + "\n\nHeaders:\n" + end.Headers.ToString(), Log.Type.OutPut);

                        return end;
                    }
                }
                else httpWebRequest.Proxy = null;


                byte[] bytes = new ASCIIEncoding().GetBytes(config.PostData);
                httpWebRequest.ContentLength = bytes.Length;


                httpWebRequest = SetOtherSetting(config, httpWebRequest);

                httpWebRequest.CookieContainer = container;

                //Add Headers
                if (config.Headers != null)
                {
                    httpWebRequest.Headers = Utils.SetHeaders(config);
                }

                //Add Cookies
                if (config.Cookies != null)
                {
                    container = Utils.SetCookies(config, container, httpWebRequest.Host);
                }

                //GetRequest
                try
                {
                    if (mange.Cookies.Count != 0)
                    {
                        container = mange.Cookies;
                        container.ToString();
                    }
                }
                catch { }
                httpWebRequest.CookieContainer = container;
                
                logger.AddMessage("Getting Response", Log.Type.Infomation);

                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);

                HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
                requestStream.Close();
                container.Add(response.Cookies);

                end = Utils.GetManage(response, container);
                end.CookiesString = Utils.GetCookiesString(container, config);

                end.StatusCode = (int)response.StatusCode;

                logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + end.SourcePage + "\n\nCookies:\n" + Utils.GetCookiesString(end.Cookies, config) + "\n\nHeaders:\n" + end.Headers.ToString() + "\nCode: " + end.StatusCode.ToString(), Log.Type.OutPut);


                return end;
            }
            catch (WebException ex)
            {
                string error = ex.Message;

                end.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;

                end.ErrorAst = true;
                end.SourcePage = "ERROR|" + error + "|" + ex.Message;
                logger.AddMessage("RequestManage OutPut\nSoucePage:\n" + end.SourcePage + "\n\nCode: " + end.StatusCode.ToString(), Log.Type.OutPut);

                return end;
            }

        }
    }
}
