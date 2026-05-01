using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MediaManager.Data.Context;

namespace MediaManager.Data.Migrations
{
    [DbContext(typeof(MediaDbContext))]
    [Migration("20260417_AddImageOtherFileAndLibrary")]
    partial class AddImageOtherFileAndLibrary
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("MediaManager.Core.Models.MediaFile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<double>("DurationSeconds")
                        .HasColumnType("REAL");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(260)
                        .HasColumnType("TEXT");

                    b.Property<long>("FileSize")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT");

                    b.Property<int>("MediaType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Rating")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Hash");

                    b.HasIndex("Path")
                        .IsUnique();

                    b.ToTable("MediaFiles", (string)null);

                    b.HasDiscriminator<int>("MediaType");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("MediaManager.Core.Models.MediaTag", b =>
                {
                    b.Property<long>("MediaFileId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("MediaFileId", "TagId");

                    b.HasIndex("TagId");

                    b.ToTable("MediaTags", (string)null);
                });

            modelBuilder.Entity("MediaManager.Core.Models.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Playlists", (string)null);
                });

            modelBuilder.Entity("MediaManager.Core.Models.PlaylistItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("MediaFileId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlaylistId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SortOrder")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("MediaFileId");

                    b.HasIndex("PlaylistId", "SortOrder");

                    b.ToTable("PlaylistItems", (string)null);
                });

            modelBuilder.Entity("MediaManager.Core.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Color")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT")
                        .HasDefaultValue("#607D8B");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags", (string)null);
                });

            modelBuilder.Entity("MediaManager.Core.Models.AudioFile", b =>
                {
                    b.HasBaseType("MediaManager.Core.Models.MediaFile");

                    b.Property<string>("Album")
                        .HasColumnType("TEXT");

                    b.Property<string>("AlbumArtPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .HasColumnType("TEXT");

                    b.Property<int>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Channels")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Codec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Genre")
                        .HasColumnType("TEXT");

                    b.Property<int>("SampleRate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TrackNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WaveformImagePath")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Year")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(0);
                });

            modelBuilder.Entity("MediaManager.Core.Models.VideoFile", b =>
                {
                    b.HasBaseType("MediaManager.Core.Models.MediaFile");

                    b.Property<string>("AudioCodec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<double>("FrameRate")
                        .HasColumnType("REAL");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ThumbnailPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<string>("VideoCodec")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.ToTable("MediaFiles", t =>
                        {
                            t.Property("BitRate")
                                .HasColumnName("VideoFile_BitRate");

                            t.Property("Title")
                                .HasColumnName("VideoFile_Title");
                        });

                    b.HasDiscriminator().HasValue(1);
                });

            modelBuilder.Entity("MediaManager.Core.Models.ImageFile", b =>
                {
                    b.HasBaseType("MediaManager.Core.Models.MediaFile");

                    b.Property<string>("Format")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasDiscriminator().HasValue(2);
                });

            modelBuilder.Entity("MediaManager.Core.Models.OtherFile", b =>
                {
                    b.HasBaseType("MediaManager.Core.Models.MediaFile");

                    b.Property<string>("Extension")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue(3);
                });

            modelBuilder.Entity("MediaManager.Core.Models.Library", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("FileCount")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IncludeAudio")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IncludeImages")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IncludeVideo")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastScannedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Recursive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ScanPath")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TotalSize")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ScanPath")
                        .IsUnique();

                    b.ToTable("Libraries", (string)null);
                });

            modelBuilder.Entity("MediaManager.Core.Models.MediaTag", b =>
                {
                    b.HasOne("MediaManager.Core.Models.MediaFile", "MediaFile")
                        .WithMany("MediaTags")
                        .HasForeignKey("MediaFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediaManager.Core.Models.Tag", "Tag")
                        .WithMany("MediaTags")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaFile");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("MediaManager.Core.Models.PlaylistItem", b =>
                {
                    b.HasOne("MediaManager.Core.Models.MediaFile", "MediaFile")
                        .WithMany("PlaylistItems")
                        .HasForeignKey("MediaFileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediaManager.Core.Models.Playlist", "Playlist")
                        .WithMany("Items")
                        .HasForeignKey("PlaylistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaFile");

                    b.Navigation("Playlist");
                });

            modelBuilder.Entity("MediaManager.Core.Models.MediaFile", b =>
                {
                    b.HasOne("MediaManager.Core.Models.Library", "Library")
                        .WithMany("MediaFiles")
                        .HasForeignKey("LibraryId");

                    b.Navigation("Library");

                    b.Navigation("MediaTags");

                    b.Navigation("PlaylistItems");
                });

            modelBuilder.Entity("MediaManager.Core.Models.Library", b =>
                {
                    b.Navigation("MediaFiles");
                });

            modelBuilder.Entity("MediaManager.Core.Models.Playlist", b =>
                {
                    b.Navigation("Items");
                });

            modelBuilder.Entity("MediaManager.Core.Models.Tag", b =>
                {
                    b.Navigation("MediaTags");
                });
#pragma warning restore 612, 618
        }
    }
}
