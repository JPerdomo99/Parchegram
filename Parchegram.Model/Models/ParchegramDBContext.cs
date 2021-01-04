using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Parchegram.Model.Models
{
    public partial class ParchegramDBContext : DbContext
    {
        public ParchegramDBContext()
        {
        }

        public ParchegramDBContext(DbContextOptions<ParchegramDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<Follow> Follow { get; set; }
        public virtual DbSet<Like> Like { get; set; }
        public virtual DbSet<LogDeletePost> LogDeletePost { get; set; }
        public virtual DbSet<LogInsertPost> LogInsertPost { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Post> Post { get; set; }
        public virtual DbSet<Share> Share { get; set; }
        public virtual DbSet<TypePost> TypePost { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserImageProfile> UserImageProfile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=sql5066.site4now.net;Database=DB_A6D47F_julian1999;User Id=DB_A6D47F_julian1999_admin;Password=julian1999");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.CommentText)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.IdPostNavigation)
                    .WithMany(p => p.Comment)
                    .HasForeignKey(d => d.IdPost)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_Post");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.Comment)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_User");
            });

            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => new { e.IdUserFollower, e.IdUserFollowing });

                entity.HasOne(d => d.IdUserFollowerNavigation)
                    .WithMany(p => p.FollowIdUserFollowerNavigation)
                    .HasForeignKey(d => d.IdUserFollower)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Follow_UserFollower");

                entity.HasOne(d => d.IdUserFollowingNavigation)
                    .WithMany(p => p.FollowIdUserFollowingNavigation)
                    .HasForeignKey(d => d.IdUserFollowing)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Follow_UserFollowing");
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => new { e.IdUser, e.IdPost });

                entity.HasOne(d => d.IdPostNavigation)
                    .WithMany(p => p.Like)
                    .HasForeignKey(d => d.IdPost)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Like_Post");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.Like)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Like_User");
            });

            modelBuilder.Entity<LogDeletePost>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(3000);
            });

            modelBuilder.Entity<LogInsertPost>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(3000);
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.MessageText)
                    .IsRequired()
                    .HasMaxLength(3000);

                entity.HasOne(d => d.IdUserReceiverNavigation)
                    .WithMany(p => p.MessageIdUserReceiverNavigation)
                    .HasForeignKey(d => d.IdUserReceiver)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_UserReceiver");

                entity.HasOne(d => d.IdUserSenderNavigation)
                    .WithMany(p => p.MessageIdUserSenderNavigation)
                    .HasForeignKey(d => d.IdUserSender)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Message_UserSender");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(3000);

                entity.Property(e => e.PathFile)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdTypePostNavigation)
                    .WithMany(p => p.Post)
                    .HasForeignKey(d => d.IdTypePost)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Post_TypePost");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.Post)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Post_User");
            });

            modelBuilder.Entity<Share>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.IdPostNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.IdPost)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Share_Post");

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany()
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Share_User");
            });

            modelBuilder.Entity<TypePost>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.CodeConfirmEmail)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.DateBirth).HasColumnType("date");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.NameUser)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserImageProfile>(entity =>
            {
                entity.Property(e => e.PathImageM)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.PathImageS)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.HasOne(d => d.IdUserNavigation)
                    .WithMany(p => p.UserImageProfile)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserImageProfile_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
