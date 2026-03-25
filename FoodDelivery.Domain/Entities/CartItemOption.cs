namespace FoodDelivery.Domain.Entities
{
    public class CartItemOption
    {
        public int Id { get; set; }

        // Liên kết với CartItem (Thuộc về món nào trong giỏ?)
        public int CartItemId { get; set; }
        public virtual CartItem? CartItem { get; set; }

        // Liên kết với ProductOption (Là Size L hay Trân châu?)
        public int ProductOptionId { get; set; }
        public virtual ProductOption? ProductOption { get; set; }
    }
}