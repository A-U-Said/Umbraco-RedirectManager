using System;

namespace RedirectManager.Backend.Models
{
    public class RegisterRedirectCommand
    {
        public string OldUrl { get; set; }
        public Guid DestinationGuid { get; set; }
    }
}
