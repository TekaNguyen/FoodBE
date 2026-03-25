namespace FoodDelivery.Domain.Entities
{
    public class OptionGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool AllowMultiple { get; set; }

        public int ProductId { get; set; }
        // 👇 THÊM DÒNG NÀY ĐỂ LIÊN KẾT NGƯỢC VỀ PRODUCT
        public virtual Product? Product { get; set; }

        // 👇👇 QUAN TRỌNG: THÊM DÒNG NÀY ĐỂ HẾT LỖI
        public virtual ICollection<ProductOption> ProductOptions { get; set; } = [];
    }
}