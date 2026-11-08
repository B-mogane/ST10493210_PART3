using Microsoft.AspNetCore.Identity;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class PasswordHashTests
{
    [Fact]
    public void PasswordHasher_VerifiesCorrectPassword_AndRejectsIncorrectPassword()
    {
        var user = new User { Email = "test@example.com" };
        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "StrongPass123!");
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, user.PasswordHash, "StrongPass123!"));
        Assert.Equal(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, user.PasswordHash, "WrongPassword"));
    }
}
