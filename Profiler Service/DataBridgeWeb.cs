using Microsoft.EntityFrameworkCore;
using Profiler_Service.Models;
using System.Web.Helpers;

namespace Profiler_Service
{
    public class DataBridgeWeb
    {
        public class DataBaseContext : DbContext
        {
            public DbSet<User> Users { get; set; }

            public DataBaseContext()
            {
                Database.EnsureCreated();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseNpgsql(
                    "Host=" + Configs.Database.Host + ";" +
                    "Port=" + Configs.Database.Port + ";" +
                    "Database=" + Configs.Database.DatabaseName + ";" +
                    "Username=" + Configs.Database.Username + ";" +
                    "Password=" + Configs.Database.Password + "");
            }
        }

        public int AddUser(User user)
        {
            int code = -1;
            User? addedUser = null;
            try
            {
                DataBaseContext context = new();
                if (context.Users.Where(element => element.Login == user.Login).Count() == 1)
                { code = 1; }
                else
                if (context.Users.Where(element => element.Login == user.Login).Count() > 1)
                { code = 2; }
                else
                {
                    addedUser = context.Users.Add(user).Entity;
                    Console.WriteLine("Added user: " + addedUser.ToString());
                    context.SaveChanges();
                    code = 0;
                }
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------ERROR IN: AddUser------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------ERROR IN: AddUser------------/");
                return code;
            }
        }

        public int ChangeUser(User user, string OldPassword)
        {
            int code = -1;
            User addedUser = null;
            try
            {
                DataBaseContext context = new();
                List<User> users = context.Users.AsNoTracking().Where(element => element.Login == user.Login).ToList();
                if (users.Count == 1)
                {
                    // Verify password HASH
                    Crypto.VerifyHashedPassword(users.Where(element => element.Login == user.Login).ElementAt(0).Password, user.Password);
                    Console.WriteLine("Changing user...");
                    Console.WriteLine("Old account:\n" + users[0].ToString());
                    addedUser = context.Users.Update(user).Entity;
                    Console.WriteLine("New account:\n" + addedUser.ToString());
                    context.SaveChanges();
                    code = 0;
                }
                else
                if (users.Count > 1)
                { code = 2; }
                else
                if (users.Count == 0)
                { code = 3; }
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------ERROR IN: ChangeUser------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------ERROR IN: ChangeUser------------/");
                return code;
            }
        }
    }
}
