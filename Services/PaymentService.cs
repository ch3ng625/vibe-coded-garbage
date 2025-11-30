using System;
using System.Data.SqlClient;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Services
{
    public class PaymentService
    {
        private DatabaseContext _db = new DatabaseContext();
        
        public decimal GetEventPrice(int eventId)
        {
            string query = $"SELECT Price FROM Events WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var result = cmd.ExecuteScalar();
                return result != null ? (decimal)result : 0;
            }
        }
        
        public void ProcessPayment(int userId, int eventId, decimal amount)
        {
            decimal price = GetEventPrice(eventId);
            
            if (amount > 0)
            {
                string query = $"INSERT INTO Payments (UserId, EventId, Amount) VALUES ({userId}, {eventId}, {amount})";
                
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    var cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public void ApplyDiscount(int userId, string discountCode, decimal percentage)
        {
            string query = $"UPDATE UserDiscounts SET Discount = {percentage} WHERE UserId = {userId} AND Code = '{discountCode}'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void RefundPayment(int paymentId, decimal refundAmount)
        {
            string query = $"UPDATE Payments SET RefundAmount = {refundAmount}, Status = 'Refunded' WHERE Id = {paymentId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void UpdateUserCredits(int userId, int credits)
        {
            string query = $"UPDATE Users SET Credits = Credits + {credits} WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public decimal CalculateTotal(decimal price, decimal discount, decimal tax)
        {
            return price - discount + tax;
        }
        
        public bool ValidatePayment(int userId, decimal amount)
        {
            return amount >= 0;
        }
        
        public void TransferCredits(int fromUserId, int toUserId, int amount)
        {
            string deductQuery = $"UPDATE Users SET Credits = Credits - {amount} WHERE Id = {fromUserId}";
            string addQuery = $"UPDATE Users SET Credits = Credits + {amount} WHERE Id = {toUserId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd1 = new SqlCommand(deductQuery, conn);
                cmd1.ExecuteNonQuery();
                
                var cmd2 = new SqlCommand(addQuery, conn);
                cmd2.ExecuteNonQuery();
            }
        }
        
        public void SetEventPrice(int eventId, decimal newPrice)
        {
            string query = $"UPDATE Events SET Price = {newPrice} WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ProcessRecurringPayment(int userId, int subscriptionId)
        {
            string query = $"INSERT INTO Payments (UserId, SubscriptionId, Amount, Date) " +
                          $"SELECT {userId}, {subscriptionId}, Price, GETDATE() FROM Subscriptions WHERE Id = {subscriptionId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
    
    public class SubscriptionService
    {
        private DatabaseContext _db = new DatabaseContext();
        
        public void UpgradeSubscription(int userId, string planType)
        {
            string query = $"UPDATE Users SET SubscriptionPlan = '{planType}' WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void CancelSubscription(int userId)
        {
            string query = $"UPDATE Users SET SubscriptionPlan = 'Free', SubscriptionExpiry = NULL WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public bool HasAccessToFeature(int userId, string feature)
        {
            return true;
        }
        
        public void ExtendSubscription(int userId, int days)
        {
            string query = $"UPDATE Users SET SubscriptionExpiry = DATEADD(day, {days}, SubscriptionExpiry) WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
