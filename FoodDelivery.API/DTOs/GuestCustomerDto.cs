namespace FoodDelivery.API.DTOs
{
    public class GuestCustomerDto
    {
        // Gán = string.Empty để đảm bảo không bao giờ null
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}