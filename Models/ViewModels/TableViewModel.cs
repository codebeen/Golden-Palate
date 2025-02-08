using Microsoft.EntityFrameworkCore;

namespace RRS.Models.ViewModels
{
    public class TableViewModel
    {
        public Reservation Reservation { get; set; }
        public Table Table { get; set; }
        [Precision(10,2)]
        public decimal BuffetPrice { get; set; }
        public string BuffetName { get; set; }
        public List<Table> Tables { get; set; }

        public List<int> ReservedTableIds { get; set; } // Store reserved table IDs
    }
}
