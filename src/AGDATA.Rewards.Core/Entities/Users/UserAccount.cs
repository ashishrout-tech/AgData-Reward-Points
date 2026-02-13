//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using AGDATA.Rewards.Core.UserAggregate;

//namespace Project.Domain.Entities.Users
//{
//    public class UserAccount
//    {
//        public Guid Id { get; }
//        public Guid UserId { get; }
//        public int Points { get; private set; }

//        public User User { get; private set; } = null!;

//        private UserAccount() { }

//        public UserAccount(Guid userId)
//        {
//            if (userId == Guid.Empty)
//                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

//            Id = Guid.NewGuid();
//            UserId = userId;
//            Points = 0;
//        }

//        public void AddPoints(int points)
//        {
//            Points += points;
//        }

//        public void RedeemPoints(int points)
//        {
//            if (points <= 0)
//                throw new ArgumentOutOfRangeException(nameof(points), "Points to redeem must be positive.");

//            if (points > Points)
//                throw new InvalidOperationException("Cannot redeem more points than available.");

//            Points -= points;
//        }
//    }
//}
