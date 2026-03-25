using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Infrastructure.Persistence;
using FoodDelivery.Domain.Entities;
using System.Collections.Concurrent;
using System.Threading;

namespace FoodDelivery.API.Hubs
{
    public class ChatHub(AppDbContext context) : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _onlineUsers = new();
        // 👇 1. KHÓA: Chặn việc Join và Send chạy song song (Nguyên nhân chính gây 2 dòng)
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Staff") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminChannel");
            }
            await base.OnConnectedAsync();
        }

        // --- HÀM JOIN: XỬ LÝ GỘP NGƯỢC ---
        public async Task JoinConversation(string guestId, string nickname, string info)
        {
            // 👇 LẤY KHÓA: Nếu SendMessage đang chạy, Join phải chờ (và ngược lại)
            var lockObj = _userLocks.GetOrAdd(guestId, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                Console.WriteLine($"[DEBUG-JOIN] Xử lý cho ID: {guestId} | Tên: {nickname}");

                _onlineUsers[Context.ConnectionId] = guestId;
                await Groups.AddToGroupAsync(Context.ConnectionId, guestId);

                // 1. Tìm tài khoản CŨ trong quá khứ (Dựa theo Email/SĐT)
                // Lưu ý: Chỉ tìm nếu có info hợp lệ
                Conversation? oldOfficialConv = null;
                if (!string.IsNullOrEmpty(info))
                {
                    oldOfficialConv = await context.Conversations
                        .FirstOrDefaultAsync(c => c.EmailOrPhone == info);
                }

                // 2. Tìm tài khoản HIỆN TẠI (Dựa theo ID mà Client đang dùng)
                var currentConv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);

                // --- TRƯỜNG HỢP A: Đã tồn tại cả CŨ và MỚI (Admin đang thấy 2 dòng) ---
                if (oldOfficialConv != null && currentConv != null && oldOfficialConv.Id != currentConv.Id)
                {
                    Console.WriteLine($"[DEBUG-MERGE] Gộp từ {oldOfficialConv.Id} vào {currentConv.Id}");

                    // Chuyển hết tin nhắn từ nick cũ sang nick mới
                    var messages = await context.ChatMessages
                        .Where(m => m.ConversationId == oldOfficialConv.Id)
                        .ToListAsync();

                    foreach (var m in messages) m.ConversationId = currentConv.Id;

                    // Xóa nick cũ đi (Để Admin không thấy 2 dòng nữa)
                    var oldIdToRemove = oldOfficialConv.SessionId;
                    context.Conversations.Remove(oldOfficialConv);

                    // Cập nhật nick mới thành chính chủ
                    currentConv.UserId = nickname;
                    currentConv.EmailOrPhone = info;
                    currentConv.LastMessageAt = DateTime.UtcNow;

                    await context.SaveChangesAsync();

                    // ⚡ QUAN TRỌNG: Báo Admin xóa dòng cũ ngay lập tức
                    await Clients.Group("AdminChannel").SendAsync("RemoveGuestConversation", oldIdToRemove);
                }
                // --- TRƯỜNG HỢP B: Chỉ có nick CŨ (Khách mới mở web, chưa chat gì) ---
                else if (oldOfficialConv != null && currentConv == null)
                {
                    Console.WriteLine($"[DEBUG-MERGE] Tái sử dụng nick cũ {oldOfficialConv.Id}");

                    var oldIdToRemove = oldOfficialConv.SessionId; // Lưu ID cũ để báo xóa UI

                    // Lấy nick cũ ra xài, nhưng CẬP NHẬT ID thành ID hiện tại để khớp với Client
                    oldOfficialConv.SessionId = guestId;
                    oldOfficialConv.UserId = nickname;
                    oldOfficialConv.LastMessageAt = DateTime.UtcNow;

                    await context.SaveChangesAsync();

                    // Báo Admin xóa dòng ID cũ (vì giờ nó đã đổi sang ID mới guestId)
                    // Nếu không xóa, Admin sẽ thấy dòng cũ nằm im và dòng mới hiện ra -> thành 2 dòng
                    await Clients.Group("AdminChannel").SendAsync("RemoveGuestConversation", oldIdToRemove);
                }
                // --- TRƯỜNG HỢP C: Chưa có gì hết hoặc chỉ có nick Mới ---
                else
                {
                    if (currentConv == null)
                    {
                        // Tạo mới nếu chưa có
                        if (!string.IsNullOrEmpty(nickname) && nickname != "Khách")
                        {
                            currentConv = new Conversation
                            {
                                SessionId = guestId,
                                UserId = nickname,
                                EmailOrPhone = info,
                                StartedAt = DateTime.UtcNow,
                                LastMessageAt = DateTime.UtcNow
                            };
                            context.Conversations.Add(currentConv);
                            await context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        // Nếu đang là "Khách", cập nhật thành tên thật
                        if ((currentConv.UserId == "Khách" || currentConv.UserId != nickname) && !string.IsNullOrEmpty(nickname) && nickname != "Khách")
                        {
                            currentConv.UserId = nickname;
                            currentConv.EmailOrPhone = info;
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // Gửi tên chuẩn về cho Admin hiển thị
                var finalName = (currentConv ?? oldOfficialConv)?.UserId ?? nickname;
                await Clients.Group("AdminChannel").SendAsync("UserStatus", guestId, "Online", finalName, info);
            }
            finally
            {
                lockObj.Release(); // 👇 MỞ KHÓA
            }
        }

        public async Task JoinAdminChannel()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminChannel");
        }

        // --- HÀM SEND: CŨNG PHẢI LOCK ---
        public async Task SendMessage(string senderId, string message, string receiverId, string messageType = "text")
        {
            string guestId = senderId == "Admin" ? receiverId : senderId;

            // 👇 LẤY KHÓA: Nếu Join đang chạy, Send phải chờ
            var lockObj = _userLocks.GetOrAdd(guestId, _ => new SemaphoreSlim(1, 1));
            await lockObj.WaitAsync();

            try
            {
                var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);

                if (conv == null && senderId != "Admin")
                {
                    // Tạo mới nếu chưa có (Trường hợp khách vãng lai)
                    conv = new Conversation
                    {
                        SessionId = guestId,
                        UserId = "Khách",
                        StartedAt = DateTime.UtcNow,
                        LastMessageAt = DateTime.UtcNow
                    };
                    context.Conversations.Add(conv);
                    await context.SaveChangesAsync();
                }
                else if (conv != null)
                {
                    conv.LastMessageAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                }

                var msg = new ChatMessage
                {
                    ConversationId = conv?.Id ?? 0,
                    SenderId = senderId,
                    Content = message,
                    MessageType = messageType,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                context.ChatMessages.Add(msg);
                await context.SaveChangesAsync();

                await Clients.Group(guestId).SendAsync("ReceiveMessage", msg.Id, senderId, message, receiverId, messageType, false);
                await Clients.Group("AdminChannel").SendAsync("ReceiveMessage", msg.Id, senderId, message, receiverId, messageType, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SendMessage: " + ex.Message);
            }
            finally
            {
                lockObj.Release(); // 👇 MỞ KHÓA
            }
        }

        public async Task RecallMessage(int messageId)
        {
            try
            {
                var msg = await context.ChatMessages.Include(m => m.Conversation).FirstOrDefaultAsync(m => m.Id == messageId);
                if (msg != null)
                {
                    msg.IsDeleted = true;
                    msg.Content = "Tin nhắn đã bị thu hồi";
                    await context.SaveChangesAsync();

                    await Clients.Group("AdminChannel").SendAsync("MessageRecalled", messageId);
                    if (msg.Conversation != null)
                    {
                        await Clients.Group(msg.Conversation.SessionId).SendAsync("MessageRecalled", messageId);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Lỗi Recall: " + ex.Message); }
        }
    }
}