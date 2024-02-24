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

using System.Buffers;
using Microsoft.AspNetCore.Mvc;

namespace SagePayMvc.ActionResults
{
    /// <summary>
    /// Action Result returned when an error occurs.
    /// </summary>
    public class ErrorResult : SagePayResult
    {
        public ErrorResult(IUrlHelper urlHelper) : base(null, urlHelper)
        {
        }


        public override async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = "text/plain";
            await context.HttpContext.Response.WriteAsync("Status=ERROR" + Environment.NewLine);
            await context.HttpContext.Response.WriteAsync($"RedirectURL={BuildFailedUrl(context)}"+ Environment.NewLine);
            await context.HttpContext.Response.WriteAsync(
                "StatusDetail=An error occurred when processing the request."+ Environment.NewLine);
        }
    }
}