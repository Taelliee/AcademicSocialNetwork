using AcademicSocialNetwork.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicSocialNetwork.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Resource> Resources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Connection — self-referencing many-to-many
            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(c => c.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(c => c.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post ? Author
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post ? Group (optional)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Group)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Comment ? Author
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment ? Post
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment ? ParentComment (self-referencing)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like ? User
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Like ? Post
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message ? Sender
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message ? Conversation
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ConversationParticipant ? User
            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ConversationParticipant ? Conversation
            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group ? Creator
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupMember ? User
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // GroupMember ? Group
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event ? Organizer
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventAttendee ? User
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany(u => u.EventAttendances)
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EventAttendee ? Event
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.Attendees)
                .HasForeignKey(ea => ea.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification ? User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification ? Actor (optional)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification ? Post (optional)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Post)
                .WithMany()
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostTag ? Post
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // PostTag ? Tag
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Resource ? UploadedBy
            modelBuilder.Entity<Resource>()
                .HasOne(r => r.UploadedBy)
                .WithMany()
                .HasForeignKey(r => r.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "Administrator",        Email = "admin@uni.bg",     PasswordHash = "hashed", IsAdmin = true,                                                                                         IsOnline = false, CreatedAt = new DateTime(2024, 1, 1) },
                new User { Id = 2, FullName = "Elitsa Ilarionova",    Email = "elitsa@uni.bg",    PasswordHash = "hashed", Major = "SoftwareAndInternetTechnologies", ClassYear = 2023, Bio = "Passionate about technology.", IsOnline = true,  CreatedAt = new DateTime(2024, 1, 2) },
                new User { Id = 3, FullName = "Vlado Gospodinov", Email = "vladislav@uni.bg", PasswordHash = "hashed", Major = "SoftwareEngineering",               ClassYear = 2025, Bio = "Love coding and algorithms.",    IsOnline = true,  CreatedAt = new DateTime(2024, 1, 3) },
                new User { Id = 4, FullName = "Katerina Minkova",     Email = "katerina@uni.bg",  PasswordHash = "hashed", Major = "Mathematics",                        ClassYear = 2026, Bio = "Math enthusiast.",               IsOnline = false, CreatedAt = new DateTime(2024, 1, 4) },
                new User { Id = 5, FullName = "Martin Petrov",        Email = "martin@uni.bg",    PasswordHash = "hashed", Major = "ComputerScience",                    ClassYear = 2024, Bio = "Thesis writing season.",          IsOnline = true,  CreatedAt = new DateTime(2024, 1, 5) }
            );

            modelBuilder.Entity<Post>().HasData(
                new Post { Id = 1, Content = "Just finished my Data Structures exam! Feeling great about it. Anyone else taking it today?",                      CreatedAt = new DateTime(2026, 5, 1, 10, 0, 0),  UserId = 3, IsDeleted = false },
                new Post { Id = 2, Content = "Looking for study partners for Calculus II. Let's form a study group and meet at the library this weekend!",       CreatedAt = new DateTime(2026, 5, 1, 7, 0, 0),   UserId = 4, IsDeleted = false },
                new Post { Id = 3, Content = "Just submitted my thesis proposal! Three years of hard work finally paying off. Grateful for everyone's support.", CreatedAt = new DateTime(2026, 4, 30, 12, 0, 0), UserId = 5, IsDeleted = false }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment { Id = 1, Content = "Great job! I have mine tomorrow.",           CreatedAt = new DateTime(2026, 5, 1, 11, 0, 0),  UserId = 4, PostId = 1 },
                new Comment { Id = 2, Content = "Congratulations! That's a huge milestone!",  CreatedAt = new DateTime(2026, 4, 30, 14, 0, 0), UserId = 2, PostId = 3 },
                new Comment { Id = 3, Content = "Amazing achievement! Best of luck!",         CreatedAt = new DateTime(2026, 4, 30, 16, 0, 0), UserId = 4, PostId = 3 }
            );

            modelBuilder.Entity<Like>().HasData(
                new Like { Id = 1, UserId = 2, PostId = 1, CreatedAt = new DateTime(2026, 5, 1, 10, 30, 0) },
                new Like { Id = 2, UserId = 4, PostId = 1, CreatedAt = new DateTime(2026, 5, 1, 10, 45, 0) },
                new Like { Id = 3, UserId = 2, PostId = 2, CreatedAt = new DateTime(2026, 5, 1, 8, 0, 0) },
                new Like { Id = 4, UserId = 2, PostId = 3, CreatedAt = new DateTime(2026, 4, 30, 13, 0, 0) },
                new Like { Id = 5, UserId = 3, PostId = 3, CreatedAt = new DateTime(2026, 4, 30, 13, 30, 0) },
                new Like { Id = 6, UserId = 4, PostId = 3, CreatedAt = new DateTime(2026, 4, 30, 14, 30, 0) }
            );

            modelBuilder.Entity<Connection>().HasData(
                new Connection { Id = 1, FollowerId = 2, FollowingId = 3, Status = ConnectionStatus.Accepted, CreatedAt = new DateTime(2024, 2, 1) },
                new Connection { Id = 2, FollowerId = 2, FollowingId = 4, Status = ConnectionStatus.Accepted, CreatedAt = new DateTime(2024, 2, 2) },
                new Connection { Id = 3, FollowerId = 3, FollowingId = 2, Status = ConnectionStatus.Accepted, CreatedAt = new DateTime(2024, 2, 3) },
                new Connection { Id = 4, FollowerId = 4, FollowingId = 5, Status = ConnectionStatus.Pending,  CreatedAt = new DateTime(2024, 2, 4) }
            );

            modelBuilder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "DataStructures", UseCount = 1, CreatedAt = new DateTime(2024, 1, 1) },
                new Tag { Id = 2, Name = "StudyGroup",     UseCount = 1, CreatedAt = new DateTime(2024, 1, 1) },
                new Tag { Id = 3, Name = "CampusLife",     UseCount = 0, CreatedAt = new DateTime(2024, 1, 1) },
                new Tag { Id = 4, Name = "FinalExams",     UseCount = 0, CreatedAt = new DateTime(2024, 1, 1) }
            );

            modelBuilder.Entity<PostTag>().HasData(
                new PostTag { Id = 1, PostId = 1, TagId = 1 },
                new PostTag { Id = 2, PostId = 2, TagId = 2 }
            );

            modelBuilder.Entity<Notification>().HasData(
                new Notification { Id = 1, Type = NotificationType.Like,    Content = "Vladislav Gospodinov liked your post.",        UserId = 2, ActorId = 3, PostId = 3, IsRead = false, CreatedAt = new DateTime(2026, 4, 30, 13, 30, 0) },
                new Notification { Id = 2, Type = NotificationType.Comment, Content = "Katerina Minkova commented on your post.",     UserId = 2, ActorId = 4, PostId = 3, IsRead = false, CreatedAt = new DateTime(2026, 4, 30, 16, 0, 0) },
                new Notification { Id = 3, Type = NotificationType.Follow,  Content = "Martin Petrov sent you a connection request.", UserId = 2, ActorId = 5,              IsRead = true,  CreatedAt = new DateTime(2024, 2, 4) }
            );
        }
    }
}