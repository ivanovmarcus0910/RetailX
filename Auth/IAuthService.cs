namespace RetailX.Auth
{
    public interface IAuthService
    {
        Task<bool> SignInAsync(string username, string password);
        Task SignOutAsync();
        Task RegisterAsync(string username, string password);

    }
}
