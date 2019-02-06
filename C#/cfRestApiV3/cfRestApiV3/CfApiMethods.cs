/*
Crypto Facilities Ltd REST API V3

Copyright (c) 2018 Crypto Facilities

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace com.cryptofacilities.REST.v3
{
    public class CfApiMethods
    {
        private readonly String apiPath;
        private readonly String apiPublicKey;
        private readonly String apiPrivateKey;
        private readonly Boolean checkCertificate;

        public CfApiMethods(String apiPath, String apiPublicKey, String apiPrivateKey, Boolean checkCertificate)
        {
            this.apiPath = apiPath;
            this.apiPublicKey = apiPublicKey;
            this.apiPrivateKey = apiPrivateKey;
            this.checkCertificate = checkCertificate;
            
            // TLS 1.2+ Supported 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public CfApiMethods(String apiPath, Boolean checkCertificate) : this(apiPath, null, null, checkCertificate) { }


        #region utility methods
        // Signs a message
        private String SignMessage(String endpoint, String postData)
        {
            // Step 1: concatenate postData + endpoint
            var message = postData + endpoint;

            //Step 2: hash the result of step 1 with SHA256
            var hash256 = new SHA256Managed();
            var hash = hash256.ComputeHash(Encoding.UTF8.GetBytes(message));

            //step 3: base64 decode apiPrivateKey
            var secretDecoded = (System.Convert.FromBase64String(apiPrivateKey));

            //step 4: use result of step 3 to hash the resultof step 2 with HMAC-SHA512
            var hmacsha512 = new HMACSHA512(secretDecoded);
            var hash2 = hmacsha512.ComputeHash(hash);

            //step 5: base64 encode the result of step 4 and return
            return System.Convert.ToBase64String(hash2);

        }

        // Sends an HTTP request
        private String MakeRequest(String requestMethod, String endpoint, String postUrl = "", String postBody = "")
        {
           
            if (!checkCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            }
            using (var client = new WebClient())
            {
                var url = apiPath + endpoint + "?" + postUrl;

                //create authentication headers
                if (apiPublicKey != null && apiPrivateKey != null)
                {
                    var postData = postUrl + postBody;
                    var signature = SignMessage(endpoint, postData);
                    client.Headers.Add("APIKey", apiPublicKey);
                    client.Headers.Add("Authent", signature);
                }

                if (requestMethod == "POST" && postBody.Length > 0)
                {
                    NameValueCollection parameters = new NameValueCollection();
                    String[] bodyArray = postBody.Split('&');
                    foreach (String pair in bodyArray)
                    {
                        String[] splitPair = pair.Split('=');
                        parameters.Add(splitPair[0], splitPair[1]);
                    }

                    var response = client.UploadValues(url, "POST", parameters);
                    return Encoding.UTF8.GetString(response);
                }
                else
                {
                    return client.DownloadString(url);
                }
            }
        }
        #endregion


        #region public endpoints
        // Returns all instruments with specifications
        public String GetInstruments()
        {
            var endpoint = "/api/v3/instruments";
            return MakeRequest("GET", endpoint);
        }


        // Returns market data for all instruments
        public String GetTickers()
        {
            var endpoint = "/api/v3/tickers";
            return MakeRequest("GET", endpoint);
        }

        // Returns the entire order book for a futures
        public String GetOrderBook(String symbol)
        {
            var endpoint = "/api/v3/orderbook";
            var postUrl = "symbol=" + symbol;
            return MakeRequest("GET", endpoint, postUrl);
        }

        // Returns historical data for futures and indices
        public String GetHistory(String symbol, DateTime lastTime)
        {
            var endpoint = "/api/v3/history";
            var postUrl = "symbol=" + symbol + "&lastTime=" + lastTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return MakeRequest("GET", endpoint, postUrl);
        }

        // Returns historical data for futures and indices
        public String GetHistory(String symbol)
        {
            var endpoint = "/api/v3/history";
            var postUrl = "symbol=" + symbol;
            return MakeRequest("GET", endpoint, postUrl);
        }
        #endregion

        #region private endpoints

        // Returns key account information
        public String GetAccounts()
        {
            var endpoint = "/api/v3/accounts";
            return MakeRequest("GET", endpoint);
        }

        // Places an order
        public String SendOrder(String orderType, String symbol, String side, Decimal size, Decimal limitPrice, Decimal stopPrice = 0M)
        {
            var endpoint = "/api/v3/sendorder";
            String postBody;
            if (orderType.Equals("lmt"))
            {
                postBody = String.Format("orderType=lmt&symbol={0}&side={1}&size={2}&limitPrice={3}", symbol, side, size, limitPrice);
            }
            else if (orderType.Equals("stp"))
            {
                postBody = String.Format("orderType=stp&symbol={0}&side={1}&size={2}&limitPrice={3}&stopPrice={4}", symbol, side, size, limitPrice, stopPrice);
            }
            else
            {
                postBody = String.Empty;
            }

            return MakeRequest("POST", endpoint, String.Empty, postBody);
        }

        // Cancels an order
        public String CancelOrder(String orderId)
        {
            var endpoint = "/api/v3/cancelorder";
            var postBody = "order_id=" + orderId;
            return MakeRequest("POST", endpoint, String.Empty, postBody);
        }

        // Cancels all orders
        public String CancelAllOrders()
        {
            var endpoint = "/api/v3/cancelallorders";
            return MakeRequest("POST", endpoint);
        }

        // Dead Man Switch. Cancels all orders after X
        public String CancelAllOrdersAfter(int timeoutInSec)
        {
            var endpoint = "/api/v3/cancelallordersafter";
            var postUrl = "timeout=" + timeoutInSec;
            return MakeRequest("POST", endpoint, postUrl);
        }

        // Places or cancels orders in batch
        public String SendBatchOrder(String jsonElement)
        {
            var endpoint = "/api/v3/batchorder";
            var postBody = "json=" + jsonElement;
            return MakeRequest("POST", endpoint, String.Empty, postBody);
        }

        // Returns all open orders
        public String GetOpenOrders()
        {
            var endpoint = "/api/v3/openorders";
            return MakeRequest("GET", endpoint);
        }

        // Returns filled orders
        public String GetFills(DateTime lastFillTime)
        {
            var endpoint = "/api/v3/fills";
            var postUrl = "lastFillTime=" + lastFillTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return MakeRequest("GET", endpoint, postUrl);
        }

        // Returns filled orders
        public String GetFills()
        {
            var endpoint = "/api/v3/fills";
            return MakeRequest("GET", endpoint);
        }

        // Returns all open positions
        public String GetOpenPositions()
        {
            var endpoint = "/api/v3/openpositions";
            return MakeRequest("GET", endpoint);
        }

        // Returns the platform noticiations
        public String GetNotifications()
        {
            var endpoint = "/api/v3/notifications";
            return MakeRequest("GET", endpoint);
        }

        // Returns xbt transfers
        public String GetTransfers(DateTime lastTransferTime)
        {
            var endpoint = "/api/v3/transfers";
            var postUrl = "lastTransferTime=" + lastTransferTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            return MakeRequest("GET", endpoint, postUrl);
        }

        // Returns xbt transfers
        public String getTransfers()
        {
            var endpoint = "/api/v3/transfers";
            return MakeRequest("GET", endpoint);
        }

        #endregion
    }
}
