using Microsoft.EntityFrameworkCore;
using Profiler_Service.Models;
using System;
using System.Linq;

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
            User addedUser = null;
            try
            {
                DataBaseContext context = new();
                if (context.Users.Where(element => element.Login == user.Login).Count() > 0)
                { code = 1; }
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
                Console.WriteLine("/------------AddUser------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------AddUser------------/");
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
                if (context.Users.Where(element => element.Login == user.Login && element.Password == OldPassword).Count() > 0)
                {
                    Console.WriteLine("Changing user...");
                    Console.WriteLine("Old: " + user.ToString());
                    addedUser = context.Users.Update(user).Entity;
                    Console.WriteLine("New: " + addedUser.ToString());
                    context.SaveChanges();
                    code = 0;
                }
                else
                { code = 1; }
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("/------------ChangeUser------------\\");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("\\------------ChangeUser------------/");
                return code;
            }
        }
    }
}
