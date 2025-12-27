using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Infrastructure.Persistence;
using FoodDelivery.Domain.Entities;
using System.Collections.Concurrent;

namespace FoodDelivery.API.Hubs
{
    // Sử dụng Primary Constructor của C# 12 (biến 'context' dùng cho toàn bộ class)
    public class ChatHub(AppDbContext context) : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            // Nếu người kết nối là Admin (dựa trên Token/Role)
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminChannel");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            if (_onlineUsers.TryRemove(Context.ConnectionId, out string? guestId))
            {
                await Clients.Group("AdminChannel").SendAsync("UserStatus", guestId, "Offline");
            }
            await base.OnDisconnectedAsync(ex);
        }

        // Khách tham gia với đầy đủ thông tin
        public async Task JoinConversation(string guestId, string nickname, string info)
        {
            _onlineUsers[Context.ConnectionId] = guestId;
            await Groups.AddToGroupAsync(Context.ConnectionId, guestId);

            // Đồng bộ DB cho Conversation
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);
            if (conv == null)
            {
                context.Conversations.Add(new Conversation
                {
                    SessionId = guestId,
                    CustomName = nickname, // Dùng CustomName cho đồng bộ với AppDbContext
                    EmailOrPhone = info,
                    StartedAt = DateTime.UtcNow,
                    LastMessageAt = DateTime.UtcNow
                });
            }
            else
            {
                conv.CustomName = nickname;
                conv.EmailOrPhone = info;
            }
            await context.SaveChangesAsync();

            // Báo cho Admin biết khách này Online
            await Clients.Group("AdminChannel").SendAsync("UserStatus", guestId, "Online", nickname, info);
        }

        public async Task JoinAdminChannel() => await Groups.AddToGroupAsync(Context.ConnectionId, "AdminChannel");

        // HÀM CHÍNH: Gửi tin nhắn (Cả Text và Image dùng chung)
        public async Task SendMessage(string senderId, string message, string receiverId, string messageType = "text")
        {
            try
            {
                // GuestId là receiverId (nếu admin gửi) hoặc senderId (nếu khách gửi)
                string guestId = senderId == "Admin" ? receiverId : senderId;

                // 1. Lưu vào Database
                await SaveToDb(senderId, message, guestId, messageType);

                // 2. Gửi Realtime
                // Gửi cho nhóm khách (receiverId hoặc chính mình)
                await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, message, receiverId, messageType);

                // Nếu khách gửi, báo cho Admin
                if (senderId != "Admin")
                {
                    await Clients.Group("AdminChannel").SendAsync("ReceiveMessage", senderId, message, receiverId, messageType);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---> LỖI CHATHUB: {ex.Message}");
                throw new HubException("Server không thể xử lý tin nhắn.");
            }
        }

        // Hàm gõ chữ
        public async Task SendTyping(string guestId)
        {
            await Clients.Group("AdminChannel").SendAsync("ReceiveTyping", guestId);
        }

        // Logic lưu DB dùng chung
        private async Task SaveToDb(string senderId, string text, string guestId, string messageType)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);
            if (conv != null)
            {
                conv.LastMessageAt = DateTime.UtcNow;

                context.ChatMessages.Add(new ChatMessage
                {
                    SenderId = senderId,
                    Content = text,
                    CreatedAt = DateTime.UtcNow,
                    ConversationId = conv.Id,
                    MessageType = messageType,
                    IsRead = false
                });

                await context.SaveChangesAsync();
            }
        }

        // Trong ChatHub.cs
        public async Task JoinCustomerChat(string guestId)
        {
            // Khách hàng tham gia vào group tên là guestId của chính mình
            await Groups.AddToGroupAsync(Context.ConnectionId, guestId);
            Console.WriteLine($"Guest {guestId} joined group {guestId}");
        }
    }
}