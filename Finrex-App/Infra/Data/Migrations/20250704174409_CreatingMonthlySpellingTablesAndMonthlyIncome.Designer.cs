﻿// <auto-generated />
using System;
using Finrex_App.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Finrex_App.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250704174409_CreatingMonthlySpellingTablesAndMonthlyIncome")]
    partial class CreatingMonthlySpellingTablesAndMonthlyIncome
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.17")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Finrex_App.Domain.Entities.MonthlyIncome", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Benefits")
                        .HasColumnType("numeric");

                    b.Property<decimal>("BussinesProfit")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Freelance")
                        .HasColumnType("numeric");

                    b.Property<decimal>("MainIncome")
                        .HasColumnType("numeric");

                    b.Property<int>("Mes")
                        .HasColumnType("integer");

                    b.Property<decimal>("Other")
                        .HasColumnType("numeric");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("MIncome");
                });

            modelBuilder.Entity("Finrex_App.Domain.Entities.MonthlySpending", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Entertainment")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Groceries")
                        .HasColumnType("numeric");

                    b.Property<int>("Mes")
                        .HasColumnType("integer");

                    b.Property<decimal>("Rent")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Transportation")
                        .HasColumnType("numeric");

                    b.Property<int>("UsuarioId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Utilities")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioId");

                    b.ToTable("MSpending");
                });

            modelBuilder.Entity("Finrex_App.Domain.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AtualizadoEm")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CriadoEm")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Senha")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Finrex_App.Domain.Entities.MonthlyIncome", b =>
                {
                    b.HasOne("Finrex_App.Domain.Entities.User", "User")
                        .WithMany("MonthlyIncomes")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Finrex_App.Domain.Entities.MonthlySpending", b =>
                {
                    b.HasOne("Finrex_App.Domain.Entities.User", "User")
                        .WithMany("MonthlySpendings")
                        .HasForeignKey("UsuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Finrex_App.Domain.Entities.User", b =>
                {
                    b.Navigation("MonthlyIncomes");

                    b.Navigation("MonthlySpendings");
                });
#pragma warning restore 612, 618
        }
    }
}
