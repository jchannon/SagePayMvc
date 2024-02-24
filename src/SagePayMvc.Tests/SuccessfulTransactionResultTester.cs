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

using System;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using SagePayMvc.ActionResults;

namespace SagePayMvc.Tests {
    [TestFixture]
    public class SuccessfulTransactionResultTester {
        TestServer server;
        HttpClient httpClient;


        [SetUp]
        public void Setup() {
            this.server = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices(x => {
                        x.AddRouting();
                        x.AddControllers();
                    })
                    .Configure(x => {
                        x.UseRouting();

                        x.UseEndpoints(endpoints => { endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}"); });
                    })
            );
            this.httpClient = this.server.CreateClient();
        }

        [Test]
        public async Task Sets_content_type() {
            var res = await this.httpClient.GetAsync("test/sagepayok");
            res.Content.Headers.ContentType!.MediaType.ShouldEqual("text/plain");
        }

        [Test]
        public async Task Sets_status_ok_if_response_status_not_error() {
            var res = await this.httpClient.GetAsync("test/sagepayok");
            var body = await res.Content.ReadAsStringAsync();
            body.ShouldStartWith("Status=OK" + Environment.NewLine);
        }

        [Test]
        public async Task Sets_status_invalid_if_response_status_is_error() {
            var res = await this.httpClient.GetAsync("test/sagepayerror");
            var body = await res.Content.ReadAsStringAsync();
            body.ShouldStartWith("Status=INVALID" + Environment.NewLine);
        }

        [Test]
        public async Task Redirect_to_order_success_page_if_status_is_ok() {
            var res = await this.httpClient.GetAsync("test/sagepayok");
            var body = await res.Content.ReadAsStringAsync();

            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[1].ShouldEqual("RedirectURL=" + Configuration.Current.Protocol + "://" + Configuration.Current.NotificationHostName + "/" + Configuration.Current.SuccessController + "/" + Configuration.Current.SuccessAction + "?vendorTxCode=123");
        }

        [Test]
        public async Task Redirect_to_order_success_page_if_status_is_authenticated() {
            var res = await this.httpClient.GetAsync("test/sagepayauthenticated");
            var body = await res.Content.ReadAsStringAsync();

            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[1].ShouldEqual("RedirectURL=" + Configuration.Current.Protocol + "://" + Configuration.Current.NotificationHostName + "/" + Configuration.Current.SuccessController + "/" + Configuration.Current.SuccessAction + "?vendorTxCode=123");
        }

        [Test]
        public async Task Redirect_to_order_success_page_if_status_is_registered() {
            var res = await this.httpClient.GetAsync("test/sagepayregistered");
            var body = await res.Content.ReadAsStringAsync();

            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[1].ShouldEqual("RedirectURL=" + Configuration.Current.Protocol + "://" + Configuration.Current.NotificationHostName + "/" + Configuration.Current.SuccessController + "/" + Configuration.Current.SuccessAction + "?vendorTxCode=123");
        }

        [Test]
        public async Task Redirect_to_order_failed_page_if_status_not_one_of_ok_authenticated_registered() {
            var res = await this.httpClient.GetAsync("test/sagepayerror");
            var body = await res.Content.ReadAsStringAsync();
            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[1].ShouldEqual("RedirectURL=" + Configuration.Current.Protocol + "://" + Configuration.Current.NotificationHostName + "/" + Configuration.Current.FailedController + "/" + Configuration.Current.FailedAction + "?vendorTxCode=123");
        }
    }
}