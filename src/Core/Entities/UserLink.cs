using System.ComponentModel.DataAnnotations;

namespace Realworlddotnet.Core.Entities;

public class UserLink(string username, string followerUsername)
{
    [MaxLength(100)]
    public string Username { get; set; } = username;

    [MaxLength(100)]
    public string FollowerUsername { get; set; } = followerUsername;

    public User User { get; set; } = null!;
    public User FollowerUser { get; set; } = null!;
}
