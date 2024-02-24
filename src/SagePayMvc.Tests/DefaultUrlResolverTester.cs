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

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace SagePayMvc.Tests {
    [TestFixture]
    public class DefaultUrlResolverTester {
        TestServer server;
        HttpClient httpClient;

        [OneTimeSetUp]
        public void TestFixtureSetup() {
            Configuration.Configure(new Configuration { NotificationHostName = "foo.com" });
        }

        [OneTimeTearDown]
        public void TestFixtureTeardown() {
            Configuration.Configure(null);
        }


        public void Setup(string routeName, string pattern, object routeDefinition) {
            this.server = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices(x => {
                        x.AddRouting();
                        x.AddControllers();
                    })
                    .Configure(x => {
                        x.UseRouting();

                        x.UseEndpoints(endpoints => { endpoints.MapControllerRoute(routeName, pattern, routeDefinition); });
                    })
            );
            this.httpClient = this.server.CreateClient();
        }

        private static IEnumerable<TestCaseData> SuccessRoutePatterns() {
            yield return new TestCaseData("payment-response", "{controller}/{action}/{vendorTxCode}", new { action = "Index", vendorTxCode = "" }, "http://foo.com/PaymentResponse/Success/foo");
            yield return new TestCaseData("default", "{controller=Home}/{action=Index}", null, "http://foo.com/PaymentResponse/Success?vendorTxCode=foo");
        }

        private static IEnumerable<TestCaseData> FailureRoutePatterns() {
            yield return new TestCaseData("payment-response", "{controller}/{action}/{vendorTxCode}", new { action = "Index", vendorTxCode = "" }, "http://foo.com/PaymentResponse/Failed/foo");
            yield return new TestCaseData("default", "{controller=Home}/{action=Index}", null, "http://foo.com/PaymentResponse/Failed?vendorTxCode=foo");
        }

        private static IEnumerable<TestCaseData> NotifyRoutePatterns() {
            yield return new TestCaseData("payment-response", "{controller}/{action}/{vendorTxCode}", new { action = "Index", vendorTxCode = "" }, "http://foo.com/PaymentResponse");
        }

        private static IEnumerable<TestCaseData> HttpsRoutePatterns() {
            yield return new TestCaseData("payment-response", "{controller}/{action}/{vendorTxCode}", new { action = "Index", vendorTxCode = "" }, "https://foo.com/PaymentResponse/Success/foo");
            yield return new TestCaseData("default", "{controller=Home}/{action=Index}", null, "https://foo.com/PaymentResponse/Success?vendorTxCode=foo");
        }

        [Test, TestCaseSource(nameof(SuccessRoutePatterns))]
        public async Task Resolves_successful_url(string routeName, string pattern, object routeDefinition, string expectedResult) {
            Setup(routeName, pattern, routeDefinition);

            var res = await this.httpClient.GetAsync("/test/getsuccessurl");
            var body = await res.Content.ReadAsStringAsync();

            body.ShouldEqual(expectedResult);
        }

        [Test, TestCaseSource(nameof(FailureRoutePatterns))]
        public async Task Resolves_failed_url(string routeName, string pattern, object routeDefinition, string expectedResult) {
            Setup(routeName, pattern, routeDefinition);

            var res = await this.httpClient.GetAsync("/test/getfailureurl");
            var body = await res.Content.ReadAsStringAsync();

            body.ShouldEqual(expectedResult);
        }

        [Test, TestCaseSource(nameof(NotifyRoutePatterns))]
        public async Task Resolves_notification_url(string routeName, string pattern, object routeDefinition, string expectedResult) {
            Setup(routeName, pattern, routeDefinition);

            var res = await this.httpClient.GetAsync("/test/getnotifyurl");
            var body = await res.Content.ReadAsStringAsync();

            body.ShouldEqual(expectedResult);
        }

        [Test]
        public void Uses_raw_notification_url_if_notification_controller_null() {
        }

        [Test, TestCaseSource(nameof(HttpsRoutePatterns))]
        public async Task Uses_https(string routeName, string pattern, object routeDefinition, string expectedResult) {
            Configuration.Current.Protocol = "https";
            Setup(routeName, pattern, routeDefinition);

            var res = await this.httpClient.GetAsync("/test/getsuccessurl");
            var body = await res.Content.ReadAsStringAsync();

            body.ShouldEqual(expectedResult);

            Configuration.Current.Protocol = "http";
        }
    }
}