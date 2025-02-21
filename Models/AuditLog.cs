namespace RRS.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public string Activity {  get; set; }
        public string Status { get; set; }

        public User User { get; set; }
    }
}
