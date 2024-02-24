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

using Microsoft.AspNetCore.Mvc;
using SagePayMvc.Internal;

namespace SagePayMvc
{
    /// <summary>
    /// Default ITransactionRegistrar implementation
    /// </summary>
    public class TransactionRegistrar : ITransactionRegistrar
    {
        readonly Configuration configuration;
        readonly IUrlResolver urlResolver;
        readonly IHttpRequestSender requestSender;

        /// <summary>
        /// Creates a new instance of the TransactionRegistrar using the configuration specified in teh web.conf, the default URL Resolver and an HTTP Request Sender.
        /// </summary>
        public TransactionRegistrar() : this(Configuration.Current, UrlResolver.Current, new HttpRequestSender())
        {
        }

        /// <summary>
        /// Creates a new instance of the TransactionRegistrar
        /// </summary>
        public TransactionRegistrar(Configuration configuration, IUrlResolver urlResolver,
            IHttpRequestSender requestSender)
        {
            this.configuration = configuration;
            this.requestSender = requestSender;
            this.urlResolver = urlResolver;
        }

        public TransactionRegistrationResponse Send(IUrlHelper urlHelper, string vendorTxCode, ShoppingBasket basket,
            Address billingAddress, Address deliveryAddress, string customerEmail,
            PaymentFormProfile paymentFormProfile = PaymentFormProfile.Normal, string currencyCode = "GBP",
            MerchantAccountType accountType = MerchantAccountType.Ecommerce, TxType txType = TxType.Payment)
        {
            var sagePayUrl = configuration.RegistrationUrl;
            var notificationUrl = urlResolver.BuildNotificationUrl(urlHelper);

            var registration = new TransactionRegistration(
                vendorTxCode, basket, notificationUrl,
                billingAddress, deliveryAddress, customerEmail,
                configuration.VendorName,
                paymentFormProfile, currencyCode, accountType, txType);

            var serializer = new HttpPostSerializer();
            var postData = serializer.Serialize(registration);

            var response = requestSender.SendRequest(sagePayUrl, postData);

            var deserializer = new ResponseSerializer();
            return deserializer.Deserialize<TransactionRegistrationResponse>(response);
        }
    }
}