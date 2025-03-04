//using Domain.Entites;

//using Domain.Enums;
//using Infrastructure.AppContext;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Infrastructure.Seeds
//{
//    public class AdminSeed
//    {
//        private const string AdminEmail = "admin@ecommerce.com"; // Admin email
//        private const string AdminPassword = "123"; // Admin password
//        private const string AdminFirstName = "System";
//        private const string AdminLastName = "Administrator";
//        private const string DefaultDepartmentName = "Management";

//        public static async Task SeedAsync(IConfiguration configuration)
//        {
//            // Build the DbContext options
//            var dbContextBuilder = new DbContextOptionsBuilder<AppDbContext>();
//            dbContextBuilder.UseSqlServer(configuration.GetConnectionString("AppConnectionString"));

//            // Create the AppDbContext instance
//            using var context = new AppDbContext(dbContextBuilder.Options);

//            // Ensure roles are added
//            if (!context.Roles.Any())
//            {
//                await AddRoles(context);
//            }

//            // Ensure admin user is added
//            if (!context.Users.Any(user => user.Email == AdminEmail))
//            {
//                await AddAdmin(context);
//            }
//        }

//        private static async Task AddRoles(AppDbContext context)
//        {
//            // Get the role names from the Enum
//            var roleNames = Enum.GetNames(typeof(Roles));

//            foreach (var roleName in roleNames)
//            {
//                // Add the role if it doesn't exist
//                if (!context.Roles.Any(role => role.Name == roleName))
//                {
//                    var role = new IdentityRole<Guid>
//                    {
//                        Name = roleName,
//                        NormalizedName = roleName.ToUpperInvariant()
//                    };

//                    await context.Roles.AddAsync(role);
//                }
//            }

//            // Commit the changes to the database
//            await context.SaveChangesAsync();
//        }

//        private static async Task AddAdmin(AppDbContext context)
//        {
//            // 1. Create admin user
//            var adminUser = new AppUser
//            {
//                Email = AdminEmail,
//                NormalizedEmail = AdminEmail.ToUpperInvariant(),
//                UserName = AdminEmail,
//                NormalizedUserName = AdminEmail.ToUpperInvariant(),
//                EmailConfirmed = true,
//                PhoneNumber = "1234567890", // Default phone number
//                PhoneNumberConfirmed = true,
//                SecurityStamp = Guid.NewGuid().ToString() // SecurityStamp
//            };

//            // Hash the password and assign it to the user
//            adminUser.PasswordHash = new PasswordHasher<AppUser>().HashPassword(adminUser, AdminPassword);

//            // Add the user to the database
//            await context.Users.AddAsync(adminUser);
//            await context.SaveChangesAsync();

//            // 2. Assign the admin role to the user
//            var adminRole = await context.Roles.FirstOrDefaultAsync(role => role.Name == "Admin");
//            if (adminRole != null)
//            {
//                await context.UserRoles.AddAsync(new IdentityUserRole<Guid>
//                {
//                    RoleId = adminRole.Id,
//                    UserId = adminUser.Id
//                });
//                await context.SaveChangesAsync();
//            }

//            // 3. Create the first post (admin's post)
//            var firstPost = new Post
//            {
//                Title = "Welcome to the Blog!",
//                Content = "This is the first post on the blog. Feel free to edit or add more posts.",
//                CreatedBy = adminUser.UserName,
//                CreatedDate = DateTime.UtcNow,
//                UserId = adminUser.Id // Link the post to the admin user
//            };

//            // Add the first post to the database
//            await context.Posts.AddAsync(firstPost);
//            await context.SaveChangesAsync();
//        }
//    }
//}
