#region License

// Copyright 2009 The Sixth Form College Farnborough (http://www.farnborough.ac.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/SagePayMvc

#endregion


using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SagePayMvc.ActionResults;

namespace SagePayMvc.Tests {
    public class TestController : Controller {
        readonly IHttpRequestSender httpRequestSender;
        // public TestController(MockHttpContext context) {
        // 	
        // 	ControllerContext = new ControllerContext(context.Object);
        // }

        public TestController(IHttpRequestSender httpRequestSender) {
            this.httpRequestSender = httpRequestSender;
            // ControllerContext = new ControllerContext {
            // 	HttpContext = context
            // };
        }

        [Route("/test/getsuccessurl")]
        public IResult GetSuccessUrl() {
            var resolver = new DefaultUrlResolver();
            var url = resolver.BuildSuccessfulTransactionUrl(this.Url, "foo");
            return Results.Text(url);
        }

        [Route("/test/getfailureurl")]
        public IResult GetFailureUrl() {
            var resolver = new DefaultUrlResolver();
            var url = resolver.BuildFailedTransactionUrl(this.Url, "foo");
            return Results.Text(url);
        }

        [Route("/test/getnotifyurl")]
        public IResult GetNotifyUrl() {
            var resolver = new DefaultUrlResolver();
            var url = resolver.BuildNotificationUrl(this.Url);
            return Results.Text(url);
        }

        [Route("/test/error")]
        public ActionResult Error() {
            var result = new ErrorResult(this.Url);
            return result;
        }

        [Route("/test/invalid")]
        public ActionResult Invalid() {
            var result = new InvalidSignatureResult("123", this.Url);
            return result;
        }

        [Route("/test/transactionnotfound")]
        public ActionResult TransactionNotFound() {
            var result = new TransactionNotFoundResult("123", this.Url);
            return result;
        }

        [Route("/test/sagepayok")]
        public ActionResult SagePayOk() {
            var sagepayResponse = new SagePayResponse() { Status = ResponseType.Ok };
            var result = new ValidOrderResult("123", sagepayResponse, this.Url);
            return result;
        }

        [Route("/test/sagepayerror")]
        public ActionResult SagePayError() {
            var sagepayResponse = new SagePayResponse() { Status = ResponseType.Error };
            var result = new ValidOrderResult("123", sagepayResponse, this.Url);
            return result;
        }

        [Route("/test/sagepayauthenticated")]
        public ActionResult SagePayAuthenticated() {
            var sagepayResponse = new SagePayResponse() { Status = ResponseType.Authenticated };
            var result = new ValidOrderResult("123", sagepayResponse, this.Url);
            return result;
        }

        [Route("/test/sagepayregistered")]
        public ActionResult SagePayRegistered() {
            var sagepayResponse = new SagePayResponse() { Status = ResponseType.Registered };
            var result = new ValidOrderResult("123", sagepayResponse, this.Url);
            return result;
        }

        [Route("/test/sagepaytransaction")]
        public IResult SagePayTransaction(string culture, string currencyCode, string profile, string transactionType) {
            if (!string.IsNullOrWhiteSpace(culture)) {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);
            }

            var paymentProfile = PaymentFormProfile.Normal;
            if (!string.IsNullOrWhiteSpace(profile)) {
                paymentProfile = Enum.Parse<PaymentFormProfile>(profile);
            }

            var txType = TxType.Payment;
            if (!string.IsNullOrWhiteSpace(transactionType)) {
                txType = Enum.Parse<TxType>(transactionType);
            }

            var deliveryAddress = new Address {
                Surname = "delivery-surname",
                Firstnames = "delivery-firstname",
                Address1 = "delivery-address1",
                Address2 = "delivery-address2",
                City = "delivery-city",
                PostCode = "delivery-postcode",
                State = "delivery-state",
                Phone = "delivery-phone",
                Country = "delivery-country"
            };

            var billingAddress = new Address {
                Surname = "Surname",
                Firstnames = "Firstname",
                Address1 = "Address1",
                Address2 = "Address2",
                City = "City",
                PostCode = "postcode",
                Country = "country",
                State = "state",
                Phone = "phone"
            };

            var basket = new ShoppingBasket("My basket") {
                new BasketItem(1, "foo", 10.5m, 2.5m)
            };

            var config = new Configuration { VendorName = "TestVendor" };

            var registration = new TransactionRegistrar(config, UrlResolver.Current, httpRequestSender);
            var transactionRegistrationResponse = registration.Send(this.Url, "foo", basket, billingAddress, deliveryAddress, "email@address.com", paymentProfile, string.IsNullOrWhiteSpace(currencyCode) ? "GBP" : currencyCode, txType: txType);

            return Results.Ok();
        }
    }
}