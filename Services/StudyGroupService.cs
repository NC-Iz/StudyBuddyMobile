using Microsoft.EntityFrameworkCore;
using StudyBuddyMobile.Data;
using StudyBuddyMobile.Models;

namespace StudyBuddyMobile.Services
{
    public class StudyGroupService
    {
        // CREATE - Create new study group
        public async Task<(bool Success, string Message)> CreateGroupAsync(string name, string description, string subject, int maxMembers, bool isPublic, int creatorUserId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var studyGroup = new StudyGroup
                {
                    Name = name,
                    Description = description,
                    Subject = subject,
                    MaxMembers = maxMembers,
                    IsPublic = isPublic,
                    CreatedBy = creatorUserId,
                    CreatedDate = DateTime.Now
                };

                context.StudyGroups.Add(studyGroup);
                await context.SaveChangesAsync();

                // Add creator as first member with Creator role
                var creatorMember = new StudyGroupMember
                {
                    StudyGroupId = studyGroup.Id,
                    UserId = creatorUserId,
                    Role = "Creator",
                    JoinedDate = DateTime.Now
                };

                context.StudyGroupMembers.Add(creatorMember);
                await context.SaveChangesAsync();

                return (true, "Study group created successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create group: {ex.Message}");
            }
        }

        // READ - Get all public groups (for browsing)
        public async Task<List<StudyGroup>> GetPublicGroupsAsync()
        {
            try
            {
                using var context = new StudyBuddyDbContext();
                return await context.StudyGroups
                    .Include(sg => sg.Creator)
                    .Include(sg => sg.Members)
                    .Where(sg => sg.IsPublic == true)
                    .OrderByDescending(sg => sg.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<StudyGroup>();
            }
        }

        // READ - Get groups user is member of
        public async Task<List<StudyGroup>> GetUserGroupsAsync(int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();
                return await context.StudyGroupMembers
                    .Include(sgm => sgm.StudyGroup)
                    .ThenInclude(sg => sg.Creator)
                    .Include(sgm => sgm.StudyGroup.Members)
                    .Where(sgm => sgm.UserId == userId)
                    .Select(sgm => sgm.StudyGroup)
                    .OrderByDescending(sg => sg.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                return new List<StudyGroup>();
            }
        }

        // READ - Get group details with members and messages
        public async Task<StudyGroup?> GetGroupDetailsAsync(int groupId, int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();
                return await context.StudyGroups
                    .Include(sg => sg.Creator)
                    .Include(sg => sg.Members)
                    .ThenInclude(m => m.User)
                    .Include(sg => sg.Messages)
                    .ThenInclude(m => m.User)
                    .FirstOrDefaultAsync(sg => sg.Id == groupId);
            }
            catch
            {
                return null;
            }
        }

        // UPDATE - Edit group (only for creator/admin)
        public async Task<(bool Success, string Message)> UpdateGroupAsync(int groupId, int userId, string name, string description, string subject, int maxMembers, bool isPublic)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var group = await context.StudyGroups
                    .Include(sg => sg.Members)
                    .FirstOrDefaultAsync(sg => sg.Id == groupId);

                if (group == null)
                {
                    return (false, "Group not found");
                }

                // Check if user has permission
                var userMember = group.Members?.FirstOrDefault(m => m.UserId == userId);
                if (userMember == null || (userMember.Role != "Creator" && userMember.Role != "Admin"))
                {
                    return (false, "You don't have permission to edit this group");
                }

                group.Name = name;
                group.Description = description;
                group.Subject = subject;
                group.MaxMembers = maxMembers;
                group.IsPublic = isPublic;

                await context.SaveChangesAsync();
                return (true, "Group updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update group: {ex.Message}");
            }
        }

        // DELETE - Delete group (only creator)
        public async Task<(bool Success, string Message)> DeleteGroupAsync(int groupId, int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var group = await context.StudyGroups
                    .Include(sg => sg.Members)
                    .FirstOrDefaultAsync(sg => sg.Id == groupId);

                if (group == null)
                {
                    return (false, "Group not found");
                }

                // Check if user is creator
                var userMember = group.Members?.FirstOrDefault(m => m.UserId == userId);
                if (userMember?.Role != "Creator")
                {
                    return (false, "Only the group creator can delete the group");
                }

                context.StudyGroups.Remove(group);
                await context.SaveChangesAsync();

                return (true, "Group deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete group: {ex.Message}");
            }
        }

        // JOIN - Join a group
        public async Task<(bool Success, string Message)> JoinGroupAsync(int groupId, int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var group = await context.StudyGroups
                    .Include(sg => sg.Members)
                    .FirstOrDefaultAsync(sg => sg.Id == groupId);

                if (group == null)
                {
                    return (false, "Group not found");
                }

                // Check if already a member
                if (group.Members?.Any(m => m.UserId == userId) == true)
                {
                    return (false, "You are already a member of this group");
                }

                // Check if group is full
                if (group.Members?.Count >= group.MaxMembers)
                {
                    return (false, "This group is full");
                }

                var newMember = new StudyGroupMember
                {
                    StudyGroupId = groupId,
                    UserId = userId,
                    Role = "Member",
                    JoinedDate = DateTime.Now
                };

                context.StudyGroupMembers.Add(newMember);
                await context.SaveChangesAsync();

                return (true, "Successfully joined the study group!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to join group: {ex.Message}");
            }
        }

        // LEAVE - Leave a group
        public async Task<(bool Success, string Message)> LeaveGroupAsync(int groupId, int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var membership = await context.StudyGroupMembers
                    .FirstOrDefaultAsync(sgm => sgm.StudyGroupId == groupId && sgm.UserId == userId);

                if (membership == null)
                {
                    return (false, "You are not a member of this group");
                }

                // If creator is leaving, check if there are other members
                if (membership.Role == "Creator")
                {
                    var otherMembers = await context.StudyGroupMembers
                        .Where(sgm => sgm.StudyGroupId == groupId && sgm.UserId != userId)
                        .ToListAsync();

                    if (otherMembers.Any())
                    {
                        return (false, "As the creator, you cannot leave the group while there are other members. Please transfer ownership or remove all members first.");
                    }
                }

                context.StudyGroupMembers.Remove(membership);
                await context.SaveChangesAsync();

                return (true, "Successfully left the study group");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to leave group: {ex.Message}");
            }
        }

        // CHAT - Send message to group
        public async Task<(bool Success, string Message)> SendMessageAsync(int groupId, int userId, string message)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                // Check if user is a member
                var isMember = await context.StudyGroupMembers
                    .AnyAsync(sgm => sgm.StudyGroupId == groupId && sgm.UserId == userId);

                if (!isMember)
                {
                    return (false, "You must be a member to send messages");
                }

                var groupMessage = new GroupMessage
                {
                    StudyGroupId = groupId,
                    UserId = userId,
                    Message = message.Trim(),
                    SentDate = DateTime.Now
                };

                context.GroupMessages.Add(groupMessage);
                await context.SaveChangesAsync();

                return (true, "Message sent successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send message: {ex.Message}");
            }
        }

        // Get statistics
        public async Task<(int TotalGroups, int MyGroups, int CreatedGroups)> GetGroupStatsAsync(int userId)
        {
            try
            {
                using var context = new StudyBuddyDbContext();

                var totalGroups = await context.StudyGroups.Where(sg => sg.IsPublic).CountAsync();
                var myGroups = await context.StudyGroupMembers.Where(sgm => sgm.UserId == userId).CountAsync();
                var createdGroups = await context.StudyGroups.Where(sg => sg.CreatedBy == userId).CountAsync();

                return (totalGroups, myGroups, createdGroups);
            }
            catch
            {
                return (0, 0, 0);
            }
        }

        public void Dispose()
        {
            // No need to dispose anything since we use using statements
        }
    }
}