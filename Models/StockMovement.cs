using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public enum MovementType
    {
        In,
        Out
    }

    public class StockMovement
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public Product Product { get; set; }
    }
}