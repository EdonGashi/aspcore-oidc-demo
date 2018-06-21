using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthServer.Models
{
    public class AuthorizeViewModel
    {
        [Display(Name = "Application")]
        public string ApplicationName { get; set; }

        [BindNever]
        public string RequestId { get; set; }

        [Display(Name = "Scope")]
        public string Scope { get; set; }

        public string[] ScopesWithoutOpenid
            => Scope?
                   .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                   .Where(s => s != OpenIdConnectConstants.Scopes.OpenId)
                   .ToArray() ?? new string[0];
    }
}
