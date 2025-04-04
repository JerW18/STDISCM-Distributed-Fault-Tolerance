namespace Authentication_Service.Model
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public string RefreshToken { get; set; }
        public string UserId { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
    }
}
