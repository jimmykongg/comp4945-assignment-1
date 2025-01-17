using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuizApp.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Category> Categories { get; set; }
    
    public virtual DbSet<QuizRoom> QuizRooms { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("answers_pkey");

            entity.ToTable("answers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.RightAnswer).HasColumnName("right_answer");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("answers_quiz_id_fkey");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("app_user_pkey");

            entity.ToTable("app_user");

            entity.HasIndex(e => e.Username, "app_user_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(8)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_pkey");

            entity.ToTable("media");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasColumnName("file_path");
            entity.Property(e => e.MediaType)
                .HasMaxLength(50)
                .HasColumnName("media_type");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quizzes_pkey");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MediaId).HasColumnName("media_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_category");

            entity.HasOne(d => d.Media).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.MediaId)
                .HasConstraintName("quizzes_media_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
