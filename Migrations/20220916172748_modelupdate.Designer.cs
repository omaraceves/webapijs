﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UserDeviceApi.Context;

#nullable disable

namespace UserDeviceApi.Migrations
{
    [DbContext(typeof(UserDevicesDB))]
    [Migration("20220916172748_modelupdate")]
    partial class modelupdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc.1.22426.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("UserDeviceApi.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("UserDeviceApi.Model.UserDevice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("DeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("DeviceType")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserDevices");
                });

            modelBuilder.Entity("UserDeviceApi.Model.UserDeviceCode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("ExpirationDate")
                        .HasColumnType("bigint");

                    b.Property<Guid>("UserDeviceId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserDeviceId")
                        .IsUnique();

                    b.ToTable("UserDeviceCodes");
                });

            modelBuilder.Entity("UserDeviceApi.Model.UserDevice", b =>
                {
                    b.HasOne("UserDeviceApi.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("UserDeviceApi.Model.UserDeviceCode", b =>
                {
                    b.HasOne("UserDeviceApi.Model.UserDevice", null)
                        .WithOne("UserDeviceCode")
                        .HasForeignKey("UserDeviceApi.Model.UserDeviceCode", "UserDeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("UserDeviceApi.Model.UserDevice", b =>
                {
                    b.Navigation("UserDeviceCode")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
