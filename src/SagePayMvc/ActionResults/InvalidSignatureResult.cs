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

namespace SagePayMvc.ActionResults
{
    /// <summary>
    /// Action result used when an invalid signature is returned from SagePay.
    /// </summary>
    public class InvalidSignatureResult : SagePayResult
    {
        public InvalidSignatureResult(string vendorTxCode, IUrlHelper urlHelper) : base(vendorTxCode, urlHelper)
        {
        }


        public override async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "text/plain";
            await context.HttpContext.Response.WriteAsync("Status=INVALID"+Environment.NewLine);
            await context.HttpContext.Response.WriteAsync($"RedirectURL={BuildFailedUrl(context)}"+Environment.NewLine);
            await context.HttpContext.Response.WriteAsync(
                "StatusDetail=Cannot match the MD5 Hash. Order might be tampered with."+Environment.NewLine);
        }
    }
}