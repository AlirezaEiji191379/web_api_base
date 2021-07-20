﻿// <auto-generated />
using System;
using Event_Creator.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Event_Creator.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.8")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Event_Creator.models.Book", b =>
                {
                    b.Property<long>("BookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BookName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("CategoryId")
                        .HasColumnType("bigint");

                    b.Property<bool>("Exchangable")
                        .HasColumnType("bit");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<string>("PublisherName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("addedDate")
                        .HasColumnType("bigint");

                    b.Property<int>("imageCount")
                        .HasColumnType("int");

                    b.Property<long>("views")
                        .HasColumnType("bigint");

                    b.HasKey("BookId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("books");
                });

            modelBuilder.Entity("Event_Creator.models.Category", b =>
                {
                    b.Property<long>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ParentId")
                        .HasColumnType("int");

                    b.HasKey("CategoryId");

                    b.HasIndex("CategoryName")
                        .IsUnique();

                    b.ToTable("categories");
                });

            modelBuilder.Entity("Event_Creator.models.Exchange", b =>
                {
                    b.Property<long>("ExchangeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BookName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("bookToExchangeId")
                        .HasColumnType("bigint");

                    b.HasKey("ExchangeId");

                    b.HasIndex("bookToExchangeId");

                    b.ToTable("exchanges");
                });

            modelBuilder.Entity("Event_Creator.models.LockedAccount", b =>
                {
                    b.Property<long>("LockedAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("unlockedTime")
                        .HasColumnType("bigint");

                    b.HasKey("LockedAccountId");

                    b.HasIndex("UserId");

                    b.ToTable("lockedAccounts");
                });

            modelBuilder.Entity("Event_Creator.models.RefreshToken", b =>
                {
                    b.Property<long>("RefreshTokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("JwtTokenId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<bool>("Revoked")
                        .HasColumnType("bit");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("UserAgent")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("expirationTime")
                        .HasColumnType("bigint");

                    b.Property<string>("ipAddress")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RefreshTokenId");

                    b.HasIndex("Token")
                        .IsUnique()
                        .HasFilter("[Token] IS NOT NULL");

                    b.HasIndex("UserId");

                    b.ToTable("refreshTokens");
                });

            modelBuilder.Entity("Event_Creator.models.Security.FailedLogin", b =>
                {
                    b.Property<long>("FailedLoginId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("request")
                        .HasColumnType("int");

                    b.HasKey("FailedLoginId");

                    b.HasIndex("UserId");

                    b.ToTable("failedLogins");
                });

            modelBuilder.Entity("Event_Creator.models.Security.JwtBlackList", b =>
                {
                    b.Property<long>("JwtBlackListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("jwtToken")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("JwtBlackListId");

                    b.ToTable("jwtBlackLists");
                });

            modelBuilder.Entity("Event_Creator.models.Security.PasswordChange", b =>
                {
                    b.Property<long>("PasswordChangeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("NewPassword")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("PasswordChangeId");

                    b.HasIndex("UserId");

                    b.ToTable("changePassword");
                });

            modelBuilder.Entity("Event_Creator.models.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<bool>("Enable")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("nvarchar(11)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Event_Creator.models.Verification", b =>
                {
                    b.Property<long>("VerificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Requested")
                        .HasColumnType("int");

                    b.Property<bool>("Resended")
                        .HasColumnType("bit");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.Property<int>("VerificationCode")
                        .HasColumnType("int");

                    b.Property<long>("expirationTime")
                        .HasColumnType("bigint");

                    b.Property<string>("usage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VerificationId");

                    b.HasIndex("UserId");

                    b.ToTable("verifications");
                });

            modelBuilder.Entity("Event_Creator.models.Book", b =>
                {
                    b.HasOne("Event_Creator.models.Category", "Category")
                        .WithMany("books")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Event_Creator.models.User", "user")
                        .WithMany("books")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("user");
                });

            modelBuilder.Entity("Event_Creator.models.Exchange", b =>
                {
                    b.HasOne("Event_Creator.models.Book", "bookToExchange")
                        .WithMany("exchanges")
                        .HasForeignKey("bookToExchangeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("bookToExchange");
                });

            modelBuilder.Entity("Event_Creator.models.LockedAccount", b =>
                {
                    b.HasOne("Event_Creator.models.User", "user")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("user");
                });

            modelBuilder.Entity("Event_Creator.models.RefreshToken", b =>
                {
                    b.HasOne("Event_Creator.models.User", "user")
                        .WithMany("RefreshTokens")
                        .HasForeignKey("UserId");

                    b.Navigation("user");
                });

            modelBuilder.Entity("Event_Creator.models.Security.FailedLogin", b =>
                {
                    b.HasOne("Event_Creator.models.User", "user")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("user");
                });

            modelBuilder.Entity("Event_Creator.models.Security.PasswordChange", b =>
                {
                    b.HasOne("Event_Creator.models.User", "user")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("user");
                });

            modelBuilder.Entity("Event_Creator.models.Verification", b =>
                {
                    b.HasOne("Event_Creator.models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Event_Creator.models.Book", b =>
                {
                    b.Navigation("exchanges");
                });

            modelBuilder.Entity("Event_Creator.models.Category", b =>
                {
                    b.Navigation("books");
                });

            modelBuilder.Entity("Event_Creator.models.User", b =>
                {
                    b.Navigation("books");

                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
