// <auto-generated />
using System;
using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthChecks.UI.SQLite.Storage.Migrations
{
    [DbContext(typeof(HealthChecksDb))]
    [Migration("20200410123101_initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DiscoveryService")
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.Property<string>("Uri")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.ToTable("Configurations");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckExecution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DiscoveryService")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<DateTime>("LastExecuted")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.Property<DateTime>("OnStateFrom")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Uri")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.ToTable("Executions");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckExecutionEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<int?>("HealthCheckExecutionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HealthCheckExecutionId");

                    b.ToTable("HealthCheckExecutionEntries");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckExecutionHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int?>("HealthCheckExecutionId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<DateTime>("On")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("HealthCheckExecutionId");

                    b.ToTable("HealthCheckExecutionHistories");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckFailureNotification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("HealthCheckName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.Property<bool>("IsUpAndRunning")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastNotified")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Failures");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckExecutionEntry", b =>
                {
                    b.HasOne("HealthChecks.UI.Core.Data.HealthCheckExecution", null)
                        .WithMany("Entries")
                        .HasForeignKey("HealthCheckExecutionId");
                });

            modelBuilder.Entity("HealthChecks.UI.Core.Data.HealthCheckExecutionHistory", b =>
                {
                    b.HasOne("HealthChecks.UI.Core.Data.HealthCheckExecution", null)
                        .WithMany("History")
                        .HasForeignKey("HealthCheckExecutionId");
                });
#pragma warning restore 612, 618
        }
    }
}
