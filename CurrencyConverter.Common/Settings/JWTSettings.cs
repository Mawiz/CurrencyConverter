namespace CurrencyConverter.Common.Settings
{
    public class JWTSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireInHours { get; set; }
        public int ExpireInMinutes { get; set; }
        public int TokenLifespan { get; set; }
    }
}
