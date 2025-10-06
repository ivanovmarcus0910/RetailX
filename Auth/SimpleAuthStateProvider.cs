using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetailX.Data;
using RetailX.Domain.Entities;
using System.Security.Claims;

namespace RetailX.Auth
{
    public record AuthInfo(string UserName);

    public class SimpleAuthStateProvider : AuthenticationStateProvider, IAuthService
    {
        private const string KEY = "auth_user";
        private readonly ProtectedSessionStorage _store;
        private readonly AppDbContext _db;
        private readonly PasswordHasher<User> _hasher = new();
        public SimpleAuthStateProvider(ProtectedSessionStorage store, AppDbContext db)
        {
            _store = store;
            _db = db;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var saved = await _store.GetAsync<AuthInfo>(KEY);

            if (saved.Success && saved.Value is { } user)
                return BuildState(user.UserName);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task<bool>  SignInAsync(string username, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return false;
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
            {
                // Lưu session vào ProtectedSessionStorage (mã hoá trong trình duyệt)
                await _store.SetAsync(KEY, new AuthInfo(user.Username));

                // Báo cho Blazor: "ê, user này vừa đăng nhập đó"
                NotifyAuthenticationStateChanged(Task.FromResult(BuildState(user.Username)));

                return true;
            }

            // Sai mật khẩu
            return false;
        }

        public async Task SignOutAsync()
        {
            await _store.DeleteAsync(KEY);
            var anonymous = new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));
            NotifyAuthenticationStateChanged(Task.FromResult(anonymous));

        }

        public async Task RegisterAsync(string username, string password)
        {
            if (_db.Users.Any(u => u.Username == username))
                throw new Exception("Tên đăng nhập đã tồn tại!");

            var user = new User
            {
                Username = username,
                PasswordHash = _hasher.HashPassword(null, password)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        private static AuthenticationState BuildState(string username)
       => new(new ClaimsPrincipal(
           new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "SimpleAuth")));
    }
}
