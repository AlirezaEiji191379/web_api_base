﻿// <auto-generated />
using System;
using Event_Creator.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Event_Creator.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20210604184650_sss")]
    partial class sss
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

                    b.Property<bool>("Revoked")
                        .HasColumnType("bit");

                    b.Property<string>("Token")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint");

                    b.Property<long>("expirationTime")
                        .HasColumnType("bigint");

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
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

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

                    b.Property<string>("usage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("VerificationId");

                    b.HasIndex("UserId");

                    b.ToTable("verifications");
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
                        .WithMany()
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

            modelBuilder.Entity("Event_Creator.models.Verification", b =>
                {
                    b.HasOne("Event_Creator.models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
