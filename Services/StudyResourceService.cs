using Microsoft.EntityFrameworkCore;
using StudyBuddyMobile.Data;
using StudyBuddyMobile.Models;

namespace StudyBuddyMobile.Services
{
    public class StudyResourceService
    {
        private readonly StudyBuddyDbContext _context;

        public StudyResourceService()
        {
            _context = new StudyBuddyDbContext();
        }

        // CREATE - Add new study resource
        public async Task<(bool Success, string Message)> CreateResourceAsync(string title, string? description, string subject, string resourceType, string fileName, byte[] fileContent, string contentType, int userId)
        {
            try
            {
                var resource = new StudyResource
                {
                    Title = title,
                    Description = description ?? string.Empty,
                    Subject = subject,
                    ResourceType = resourceType,
                    FileName = fileName,
                    FileContent = fileContent,
                    ContentType = contentType,
                    FileSize = fileContent?.Length ?? 0,
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };

                _context.StudyResources.Add(resource);
                await _context.SaveChangesAsync();

                return (true, "Study resource created successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create resource: {ex.Message}");
            }
        }

        // READ - Get all resources for a user - Always fresh
        public async Task<List<StudyResource>> GetUserResourcesAsync(int userId)
        {
            try
            {
                // Use fresh context to ensure latest data
                using var context = new StudyBuddyDbContext();
                return await context.StudyResources
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<StudyResource>();
            }
        }

        // READ - Get resource by ID (for user) - Always fresh from database
        public async Task<StudyResource?> GetResourceByIdAsync(int resourceId, int userId)
        {
            try
            {
                // Create new context to ensure fresh data
                using var context = new StudyBuddyDbContext();
                return await context.StudyResources
                    .FirstOrDefaultAsync(r => r.Id == resourceId && r.UserId == userId);
            }
            catch
            {
                return null;
            }
        }

        // READ - Search resources by subject
        public async Task<List<StudyResource>> SearchResourcesBySubjectAsync(int userId, string subject)
        {
            try
            {
                return await _context.StudyResources
                    .Where(r => r.UserId == userId && r.Subject.Contains(subject))
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<StudyResource>();
            }
        }

        // UPDATE - Edit resource metadata (title and description only)
        public async Task<(bool Success, string Message)> UpdateResourceAsync(int resourceId, int userId, string title, string? description, string subject)
        {
            try
            {
                // Use fresh context for update
                using var context = new StudyBuddyDbContext();
                var resource = await context.StudyResources
                    .FirstOrDefaultAsync(r => r.Id == resourceId && r.UserId == userId);

                if (resource == null)
                {
                    return (false, "Resource not found");
                }

                resource.Title = title;
                resource.Description = description ?? string.Empty;
                resource.Subject = subject;

                await context.SaveChangesAsync();
                return (true, "Resource updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update resource: {ex.Message}");
            }
        }

        // DELETE - Remove resource
        public async Task<(bool Success, string Message)> DeleteResourceAsync(int resourceId, int userId)
        {
            try
            {
                var resource = await _context.StudyResources
                    .FirstOrDefaultAsync(r => r.Id == resourceId && r.UserId == userId);

                if (resource == null)
                {
                    return (false, "Resource not found");
                }

                _context.StudyResources.Remove(resource);
                await _context.SaveChangesAsync();

                return (true, "Resource deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete resource: {ex.Message}");
            }
        }

        // Get statistics
        public async Task<(int TotalResources, int PdfCount, int VideoCount, int SubjectCount)> GetResourceStatsAsync(int userId)
        {
            try
            {
                var resources = await _context.StudyResources
                    .Where(r => r.UserId == userId)
                    .ToListAsync();

                return (
                    TotalResources: resources.Count,
                    PdfCount: resources.Count(r => r.ResourceType == "PDF"),
                    VideoCount: resources.Count(r => r.ResourceType == "Video"),
                    SubjectCount: resources.Select(r => r.Subject).Distinct().Count()
                );
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }

        // Get file content for download
        public async Task<(byte[]? FileContent, string? ContentType, string? FileName)> GetFileForDownloadAsync(int resourceId, int userId)
        {
            try
            {
                var resource = await _context.StudyResources
                    .FirstOrDefaultAsync(r => r.Id == resourceId && r.UserId == userId);

                return (resource?.FileContent, resource?.ContentType, resource?.FileName);
            }
            catch
            {
                return (null, null, null);
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}