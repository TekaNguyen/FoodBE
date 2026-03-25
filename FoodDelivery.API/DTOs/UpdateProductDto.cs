<<<<<<< HEAD
﻿using Microsoft.AspNetCore.Http;
=======
﻿using Microsoft.AspNetCore.Http; // Để dùng IFormFile
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

namespace FoodDelivery.API.DTOs;

public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? CategoryId { get; set; }
<<<<<<< HEAD
    public int? StockQuantity { get; set; }
    public List<IFormFile>? ImageFiles { get; set; }
=======

    // 👇 Đừng quên nhân vật chính của chúng ta hôm nay
    public int? StockQuantity { get; set; }

    public IFormFile? ImageFile { get; set; }
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
}