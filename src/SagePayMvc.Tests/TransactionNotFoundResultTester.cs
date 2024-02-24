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
    public class TransactionNotFoundResultTester {
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
            var res = await this.httpClient.GetAsync("test/transactionnotfound");
            res.Content.Headers.ContentType!.MediaType.ShouldEqual("text/plain");
        }

        [Test]
        public async Task Sets_status_to_invalid() {
            var res = await this.httpClient.GetAsync("test/transactionnotfound");
            var body = await res.Content.ReadAsStringAsync();
            body.ShouldStartWith("Status=INVALID" + Environment.NewLine);
        }

        [Test]
        public async Task Sets_redirectUrl() {
            var res = await this.httpClient.GetAsync("test/transactionnotfound");
            var body = await res.Content.ReadAsStringAsync();
            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[1].ShouldEqual("RedirectURL=" + Configuration.Current.Protocol + "://" + Configuration.Current.NotificationHostName + "/" + Configuration.Current.FailedController + "/" + Configuration.Current.FailedAction + "?vendorTxCode=123");
        }

        [Test]
        public async Task Sets_statusDetail() {
            var res = await this.httpClient.GetAsync("test/transactionnotfound");
            var body = await res.Content.ReadAsStringAsync();
            var output = body.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            output[2].ShouldEqual("StatusDetail=Unable to find the transaction in our database.");
        }
    }
}