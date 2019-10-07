using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpService.Options
{
    public class EmailOptions
    {
        public string SenderEmailAddress { get; set; }

        public string SenderName { get; set; }
    }

    public class GmailOptions
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    public class SendGridOptions
    {
        public string ApiKey { get; set; }
    }
}
