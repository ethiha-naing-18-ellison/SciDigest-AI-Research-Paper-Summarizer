using Api.Infrastructure.Data;

namespace Api.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ResearchDbContext context)
    {
        // Check if database is already seeded
        if (context.Papers.Any())
        {
            return; // Database has been seeded
        }

        // Add any seed data here if needed
        // For now, we'll keep it empty as the requirements don't specify seed data
        
        await context.SaveChangesAsync();
    }
}
