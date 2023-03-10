namespace MoviesApi.Models.Identity
{
    public class RoleModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
