namespace B2B.Core.Entities
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Lock { get; set; } = false;
        public string Role { get; set; } = "Agent"; // "Admin" or "Agent"
    }
}
