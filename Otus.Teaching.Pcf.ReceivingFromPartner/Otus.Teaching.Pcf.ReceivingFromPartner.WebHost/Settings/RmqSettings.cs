namespace Otus.Teaching.Pcf.ReceivingFromPartner.WebHost.Settings
{
    public class RmqSettings
    {
        public const string Section = "RmqSettings";
        public string Host { get; set; }
        public string VHost { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

    }
}
