namespace RRS.Models.ViewModels
{
    public class AuditLogsViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email {  get; set; }
        public string? UserRole { get; set; }
        public string Activity { get; set; }
        public string Status { get; set; }

        public User? User { get; set; }
    }
}
