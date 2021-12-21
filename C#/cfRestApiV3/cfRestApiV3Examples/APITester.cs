/*
Crypto Facilities Ltd REST API V3

Copyright (c) 2021 guanlong huang

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
using System.Collections.Generic;

namespace com.cryptofacilities.REST.v3.Examples
{
    class APITester
    {
        private static readonly String apiPath = "https://www.cryptofacilities.com/derivatives";
        private static readonly String apiPublicKey = "..."; //accessible on your Account page under Settings -> API Keys
        private static readonly String apiPrivateKey = "..."; //accessible on your Account page under Settings -> API Keys

        private static readonly bool checkCertificate = true; //when using the test environment, this must be set to "False"       

        static void Main(string[] args)
        {
            CfApiMethods methods;
            String result, symbol, side, orderType;
            int cancelAfterTimeout;
            Decimal size, limitPrice, stopPrice;


            /*---------------------------Public Endpoints-----------------------------------------------*/
            methods = new CfApiMethods(apiPath, checkCertificate);

            //get instruments
            result = methods.GetInstruments();
            Console.WriteLine("getInstruments:\n" + result);

            //get tickers
            result = methods.GetTickers();
            Console.WriteLine("getTickers:\n" + result);

            //get orderbook
            symbol = "PI_XBTUSD";
            result = methods.GetOrderBook(symbol);
            Console.WriteLine("getOrderBook:\n" + result); 

            //get history
            symbol = "PI_XBTUSD";
            result = methods.GetHistory(symbol, new DateTime(2016, 01, 20));
            Console.WriteLine("getHistory:\n" + result);


            /*----------------------------Private Endpoints----------------------------------------------*/
            methods = new CfApiMethods(apiPath, apiPublicKey, apiPrivateKey, checkCertificate);

            //get accounts
            result = methods.GetAccounts();
            Console.WriteLine("getAccounts:\n" + result);

            //send limit order
            orderType = "lmt";
            symbol = "PI_XBTUSD";
            side = "buy";
            size = 1.0M;
            limitPrice = 1.0M;
            result = methods.SendOrder(orderType, symbol, side, size, limitPrice);
            Console.WriteLine("sendOrder (limit):\n" + result);

            //send stop order
            orderType = "stp";
            symbol = "PI_XBTUSD";
            side = "buy";
            size = 1.0M;
            limitPrice = 1.1M;
            stopPrice = 2.0M;
            result = methods.SendOrder(orderType, symbol, side, size, limitPrice, stopPrice);
            Console.WriteLine("sendOrder (stop):\n" + result);

            // edit order
            var edit = new Dictionary<String, String>() {
                { "orderId", "5b02d8a4-1655-4409-b26d-c896b87d6df9" },
                { "size", "2" },
                { "limitPrice", "2" }
            };
            result = methods.EditOrder(edit);
            Console.WriteLine("editOrder:\n" + result);

            //cancel order
            var orderId = "5b02d8a4-1655-4409-b26d-c896b87d6df9";
            result = methods.CancelOrder(orderId);
            Console.WriteLine("cancelOrder:\n" + result);

            //batch order
            var jsonElement = @"{
                ""batchOrder"":
                    [
                        {
                            ""order"": ""send"",
                            ""order_tag"": ""1"",
                            ""orderType"": ""lmt"",
                            ""symbol"": ""PI_XBTUSD"",
                            ""side"": ""buy"",
                            ""size"": 1,
                            ""limitPrice"": 1.00,
                        },
                        {
                            ""order"": ""send"",
                            ""order_tag"": ""2"",
                            ""orderType"": ""stp"",
                            ""symbol"": ""PI_XBTUSD"",             
                            ""side"": ""buy"",
                            ""size"": 1,
                            ""limitPrice"": 2.00,
                            ""stopPrice"": 3.00,
                        },
                        {
                            ""order"": ""cancel"",
                            ""order_id"": ""b8dbe131-5104-4fcf-af90-44321b30a6b8"",
                        },
                    ],
            }";
            result = methods.SendBatchOrder(jsonElement);
            Console.WriteLine("sendBatchOrder:\n" + result);

            //cancel all orders
            result = methods.CancelAllOrders();
            Console.WriteLine("cancelAllOrders:\n" + result);

            //cancel all orders after
            cancelAfterTimeout = 5;
            result = methods.CancelAllOrdersAfter(cancelAfterTimeout);
            Console.WriteLine("cancelAllOrdersAfter:\n" + result);

            //get open orders
            result = methods.GetOpenOrders();
            Console.WriteLine("getOpenOrders:\n" + result);

            //get fills
            var lastFillTime = new DateTime(2016, 2, 1);
            result = methods.GetFills(lastFillTime);
            Console.WriteLine("getFills:\n" + result);

            //get open positions
            result = methods.GetOpenPositions();
            Console.WriteLine("getOpenPositions:\n" + result);

            //get recent orders
            result = methods.GetRecentOrders(symbol);
            Console.WriteLine("getRecentOrders:\n" + result);

            //get notificaitons
            result = methods.GetNotifications();
            Console.WriteLine("getNotifications:\n" + result);

            //get xbt transfers
            var lastTransferTime = new DateTime(2016, 2, 1);
            result = methods.GetTransfers(lastTransferTime);
            Console.WriteLine("getTransfers:\n" + result);

            Console.In.ReadLine();

        }
    }
}
