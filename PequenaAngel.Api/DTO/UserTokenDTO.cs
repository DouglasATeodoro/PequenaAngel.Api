namespace PequenaAngel.Api.DTO
{
    public class UserTokenDTO
    {
        public bool Autenticated { get; set; }        
        public DateTime Expiration { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }

    }
}
