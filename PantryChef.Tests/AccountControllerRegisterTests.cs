using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Web.Controllers;
using PantryChef.Web.Models;

namespace PantryChef.Tests;

public class AccountControllerRegisterTests
{
    [Fact]
    public async Task Register_WhenModelStateInvalid_ReturnsViewAndDoesNotCallServices()
    {
        var sut = CreateSut();
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
        sut.AccountServiceMock.Verify(
            service => service.RegisterUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        sut.SignInManagerMock.Verify(
            manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Register_WhenAccountServiceFails_ReturnsViewWithErrors()
    {
        var sut = CreateSut();
        sut.AccountServiceMock
            .Setup(service => service.RegisterUserAsync("fail@example.com", "Password1", "Test User"))
            .ReturnsAsync(Result<ApplicationUser>.Failure("Duplicate email."));

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
            error => error.ErrorMessage == "Duplicate email.");
        sut.SignInManagerMock.Verify(
            manager => manager.SignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<bool>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Register_WhenAccountServiceSucceeds_SignsInAndRedirectsHome()
    {
        var sut = CreateSut();
        var user = new ApplicationUser
        {
            Id = "identity-user-1",
            UserName = "ivan.petrenko@example.com",
            Email = "ivan.petrenko@example.com"
        };

        sut.AccountServiceMock
            .Setup(service => service.RegisterUserAsync("ivan.petrenko@example.com", "Password1", "Ivan Petrenko"))
            .ReturnsAsync(Result<ApplicationUser>.Success(user));
        sut.SignInManagerMock
            .Setup(manager => manager.SignInAsync(user, false, null))
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
        sut.SignInManagerMock.Verify(manager => manager.SignInAsync(user, false, null), Times.Once);
    }

    private static (
        AccountController Controller,
        Mock<IAccountService> AccountServiceMock,
        Mock<UserManager<ApplicationUser>> UserManagerMock,
        Mock<SignInManager<ApplicationUser>> SignInManagerMock) CreateSut()
    {
        var accountServiceMock = new Mock<IAccountService>();
        var userManagerMock = CreateUserManagerMock();
        var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);

        var controller = new AccountController(
            userManagerMock.Object,
            signInManagerMock.Object,
            accountServiceMock.Object,
            Mock.Of<ILogger<AccountController>>());

        return (controller, accountServiceMock, userManagerMock, signInManagerMock);
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
