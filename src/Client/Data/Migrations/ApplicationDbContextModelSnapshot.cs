﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Client.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799");

            modelBuilder.Entity("Client.Data.ScholarshipApplicant", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AverageGrade");

                    b.Property<DateTime>("Date");

                    b.Property<int>("Ects");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("ScholarshipApplicants");
                });
#pragma warning restore 612, 618
        }
    }
}
