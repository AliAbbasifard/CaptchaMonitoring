using System;

namespace Entities
{
    public class Captcha
    {
        public string ConnectionId { get; set; }
        public byte[] ImageBinary { get; set; }
        public string ImageHeaders { get; set; }
        public DateTime Expiration { get; set; }
    }
}
