namespace WebApplication3.Token
{
    public record CustomTokenOptions
    {
        public string Signature { get; set; } = default!;
        public int ExpireByHour { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string[] Audience { get; set; } = default!;


    }
}
