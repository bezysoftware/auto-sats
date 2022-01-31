﻿// <auto-generated />
using System;
using AutoSats.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AutoSats.Data.Migrations
{
    [DbContext(typeof(SatsContext))]
    partial class SatsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.0");

            modelBuilder.Entity("AutoSats.Data.ApplicationSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PrivateKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PublicKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ApplicationSettings");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Error")
                        .HasColumnType("TEXT");

                    b.Property<int>("ScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ScheduleId");

                    b.ToTable("ExchangeEvents");

                    b.HasDiscriminator<string>("Type");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeSchedule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Cron")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Exchange")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsPaused")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Spend")
                        .HasColumnType("TEXT");

                    b.Property<string>("SpendCurrency")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WithdrawalAddress")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("WithdrawalLimit")
                        .HasColumnType("TEXT");

                    b.Property<int>("WithdrawalType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ExchangeSchedules");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEventBuy", b =>
                {
                    b.HasBaseType("AutoSats.Data.ExchangeEvent");

                    b.Property<string>("OrderId")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Received")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("Buy");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEventCreate", b =>
                {
                    b.HasBaseType("AutoSats.Data.ExchangeEvent");

                    b.HasDiscriminator().HasValue("Create");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEventPause", b =>
                {
                    b.HasBaseType("AutoSats.Data.ExchangeEvent");

                    b.HasDiscriminator().HasValue("Pause");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEventResume", b =>
                {
                    b.HasBaseType("AutoSats.Data.ExchangeEvent");

                    b.HasDiscriminator().HasValue("Resume");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEventWithdrawal", b =>
                {
                    b.HasBaseType("AutoSats.Data.ExchangeEvent");

                    b.Property<string>("Address")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<string>("WithdrawalId")
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue("Withdraw");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeEvent", b =>
                {
                    b.HasOne("AutoSats.Data.ExchangeSchedule", "Schedule")
                        .WithMany("Events")
                        .HasForeignKey("ScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeSchedule", b =>
                {
                    b.OwnsOne("AutoSats.Data.ExchangeScheduleNotification", "Notification", b1 =>
                        {
                            b1.Property<int>("ExchangeScheduleId")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Auth")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("P256dh")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<int>("Type")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Url")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("ExchangeScheduleId");

                            b1.ToTable("ExchangeSchedules");

                            b1.WithOwner()
                                .HasForeignKey("ExchangeScheduleId");
                        });

                    b.Navigation("Notification");
                });

            modelBuilder.Entity("AutoSats.Data.ExchangeSchedule", b =>
                {
                    b.Navigation("Events");
                });
#pragma warning restore 612, 618
        }
    }
}
