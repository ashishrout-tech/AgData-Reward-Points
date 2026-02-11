using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project.Domain.Entities.Users;
using Project.Domain.Enums;
using Project.Infrastructure.Data;

namespace Project
{
    public class Program
    {
        static async Task Practice(AppDbContext context)
        {
            var user = await context.Users.FirstAsync();
            Console.WriteLine(user.ToString());

            var userOnId = await context.Users.FindAsync(5);
            //var userTwo = await context.Users.FirstAsync(user => user.Name.StartsWith("A"));

            var usersFiltered = await context.Users.Where(user => EF.Functions.Like(user.Name, $"%s%")).ToListAsync();

            var partialUsers = await context.Users
                .Where(user => user.Name.Contains('s'))
                .ToListAsync();

            var numberOfUsers = await context.Users.CountAsync(q => q.Name == "Ashish");

            var groupedUsers = context.Users
                .GroupBy(user => user.Role)
                .Where(group => group.Any(user => user.IsActive));

            foreach (var group in groupedUsers)
            {
                Console.WriteLine(group.Key);
                Console.Write(group.Count(q => q.IsActive));
                foreach (var u in group)
                {
                    Console.WriteLine(u.Name);
                }
            }

            var orderedUsers = await context.Users
                .OrderBy(q => q.Name)
                .ToListAsync();

            var maxBy = context.Users.MaxBy(q => q.Id);

            // insert
            //var newUser = new User("Ashish", "ashish@gamil.com", "EMP001", UserRole.EMPLOYEE);
            

            //await context.Users.AddAsync(newUser);
            //await context.SaveChangesAsync();
            //Console.Write(newUser.Id);

            // update
            var user1 = await context.Users.FindAsync(9);
            user1!.UpdateUserRole(UserRole.ADMIN);
            await context.SaveChangesAsync();

            var user2 = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == new Guid());
            user2!.UpdateUserRole(UserRole.ADMIN);
            context.Update(user2);
            await context.SaveChangesAsync();

            // Delete
            var user3 = await context.Users.FindAsync(9);
            context.Remove(user3);
            await context.SaveChangesAsync();

            // execute update
            await context.Users.Where(q => q.Name == "Ashish").ExecuteUpdateAsync(set => set.SetProperty(prop => prop.Name, "asdf"));
        }

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new AppDbContext(options);

            //await Practice(context);

            var teams = context.Users.ToList();

            foreach (var t in teams)
            {
                Console.WriteLine(t.Name);
            }
        }
    }
}
