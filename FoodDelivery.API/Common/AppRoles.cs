<<<<<<< HEAD
﻿namespace FoodDelivery.API.Common
{
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Chef = "Chef"; // <-- Quan trọng cho bếp
        public const string Merchant = "Merchant";
        public const string Staff = "Staff";
        public const string Shipper = "Shipper";
        public const string Customer = "Customer";
    }
=======
﻿namespace FoodDelivery.API.Common; // 👈 Sửa thành FoodDelivery.API.Common

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Merchant = "Merchant";
    public const string Staff = "Staff";
    public const string Shipper = "Shipper";
    public const string Customer = "Customer";
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
}