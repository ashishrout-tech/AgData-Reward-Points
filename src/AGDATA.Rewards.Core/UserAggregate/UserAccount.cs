namespace AGDATA.Rewards.Core.UserAggregate;

public sealed class UserAccount
{
  public Guid UserId { get; }
  public int Points { get; private set; }


  private UserAccount() { }

  internal UserAccount(Guid userId)
  {
    UserId = userId;
    Points = 0;
  }

  internal void AddPoints(int points)
  {
    if (points <= 0)
      throw new ArgumentOutOfRangeException(nameof(points), "Points to redeem must be positive.");

    if (points > 1000_000 || Points + points > 1000_000)
      throw new InvalidOperationException("Points cannot exceed 1,000,000.");

    Points += points;
  }

  internal void DeductPoints(int points)
  {
    if (points <= 0)
      throw new ArgumentOutOfRangeException(nameof(points), "Points to redeem must be positive.");

    if (points > Points)
      throw new InvalidOperationException("Cannot redeem more points than available.");

    Points -= points;
  }
}
