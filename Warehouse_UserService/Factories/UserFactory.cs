using Warehouse_UserService.Models;

namespace Warehouse_UserService.Factories
{
    public class UserFactory
    {
        public static User RandomUser(int seed = 0)
        {
            var user = new User();
            var rnd = new Random(seed);
            user.Id = rnd.Next();
            user.Username = $"RandomUsername{rnd.Next(999)}";
            user.Name = "John";
            user.Lastname = "Doe";
            user.Email = $"johndoe{rnd.Next(999)}@mail.com";

            return user;
        }
    }
}
