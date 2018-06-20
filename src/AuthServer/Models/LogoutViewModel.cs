using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AuthServer.Models
{
    public class LogoutViewModel
    {
        [BindNever]
        public string RequestId { get; set; }
    }
}