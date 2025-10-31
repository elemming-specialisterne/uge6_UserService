namespace Warehouse_UserService.Models
{
    public class RefreshToken
    {
        //public int Id { get; set; }
        public long UserId { get; set; } = 0!;

        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool Revoked { get; set; }
        public DateTime Created { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
