using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;

namespace PantryChef.Tests;

public class AccountControllerRegisterTests
{
    [Fact]
    public async Task Register_WhenModelStateIsInvalid_ReturnsViewWithoutCreatingUsers()
    {
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(dbContext);
        sut.Controller.ModelState.AddModelError("Email", "Email is required.");

        var model = new RegisterViewModel
        {
            FullName = "Invalid User",
            Email = "invalid@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        var result = await sut.Controller.Register(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        sut.UserManagerMock.Verify(
            manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
        sut.SignInManagerMock.Verify(
            manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);
        Assert.DoesNotContain(dbContext.Users, user => user.Email == model.Email);
    }

    [Theory]
    [InlineData("Duplicate email.")]
    [InlineData("Password is too weak.")]
    public async Task Register_WhenIdentityCreationFails_ReturnsViewWithIdentityError(string identityError)
    {
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(dbContext);
        sut.UserManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = identityError }));

        var model = new RegisterViewModel
        {
            FullName = "Test User",
            Email = "fail@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        var result = await sut.Controller.Register(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.True(sut.Controller.ModelState.ContainsKey(string.Empty));
        Assert.Contains(
            sut.Controller.ModelState[string.Empty]!.Errors,
            error => error.ErrorMessage == identityError);
        sut.SignInManagerMock.Verify(
            manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);
        Assert.DoesNotContain(dbContext.Users, user => user.Email == model.Email);
    }

    [Fact]
    public async Task Register_WhenIdentityCreationSucceeds_CreatesDomainUserAndRedirectsToHome()
    {
        await using var dbContext = CreateDbContext();
        var sut = CreateSut(dbContext);
        sut.UserManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((identityUser, _) => identityUser.Id = "identity-user-1")
            .ReturnsAsync(IdentityResult.Success);
        sut.SignInManagerMock
            .Setup(manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var model = new RegisterViewModel
        {
            FullName = "Ivan Petrenko",
            Email = "ivan.petrenko@example.com",
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        var result = await sut.Controller.Register(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);

        var domainUser = Assert.Single(dbContext.Users.Where(user => user.Email == model.Email));
        Assert.Equal("identity-user-1", domainUser.IdentityUserId);
        Assert.Equal(model.FullName, domainUser.Name);
        Assert.Equal("IDENTITY_MANAGED", domainUser.Password);
        Assert.Equal(2000, domainUser.CalorieGoals);
        Assert.Equal("none", domainUser.Allergies);

        sut.SignInManagerMock.Verify(
            manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), false, null),
            Times.Once);
    }

    [Fact]
    public async Task Register_WhenDomainUserWithSameEmailExists_LinksExistingDomainUser()
    {
        await using var dbContext = CreateDbContext();
        var existingDomainUser = new User
        {
            Email = "existing.profile@example.com",
            Password = "legacy-password",
            Name = "Existing Profile",
            CalorieGoals = 1800,
            Allergies = "peanuts"
        };
        await dbContext.Users.AddAsync(existingDomainUser);
        await dbContext.SaveChangesAsync();

        var sut = CreateSut(dbContext);
        sut.UserManagerMock
            .Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((identityUser, _) => identityUser.Id = "identity-user-2")
            .ReturnsAsync(IdentityResult.Success);
        sut.SignInManagerMock
            .Setup(manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var model = new RegisterViewModel
        {
            FullName = "Ignored Name",
            Email = existingDomainUser.Email,
            Password = "Password1",
            ConfirmPassword = "Password1"
        };

        var result = await sut.Controller.Register(model);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);

        var usersWithSameEmail = dbContext.Users.Where(user => user.Email == existingDomainUser.Email).ToList();
        Assert.Single(usersWithSameEmail);

        var updatedDomainUser = usersWithSameEmail[0];
        Assert.Equal(existingDomainUser.Id, updatedDomainUser.Id);
        Assert.Equal("identity-user-2", updatedDomainUser.IdentityUserId);
        Assert.Equal("Existing Profile", updatedDomainUser.Name);
    }

    private static PantryChefDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PantryChefDbContext>()
            .UseInMemoryDatabase($"PantryChefTests-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new PantryChefDbContext(options);
    }

    private static (
        AccountController Controller,
        Mock<UserManager<ApplicationUser>> UserManagerMock,
        Mock<SignInManager<ApplicationUser>> SignInManagerMock) CreateSut(PantryChefDbContext dbContext)
    {
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        var controller = new AccountController(
            userManagerMock.Object,
            signInManagerMock.Object,
            dbContext,
            Mock.Of<ILogger<AccountController>>());

        return (controller, userManagerMock, signInManagerMock);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
    {
        return new Mock<SignInManager<ApplicationUser>>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<ApplicationUser>>());
    }
}
