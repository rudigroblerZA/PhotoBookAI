using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoBook.PhotoService.Services;

public class MetadataExtractionService : IMetadataExtractionService
{
    private readonly ILogger<MetadataExtractionService> _logger;

    public MetadataExtractionService(ILogger<MetadataExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<(int width, int height, DateTime? dateTaken, string? location, int orientation)>
        ExtractMetadataAsync(Stream imageStream)
    {
        await Task.CompletedTask;

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(imageStream);

            int width = 0;
            int height = 0;
            DateTime? dateTaken = null;
            string? location = null;
            int orientation = 1;

            // Extract image dimensions
            var jpegDirectory = directories.OfType<MetadataExtractor.Formats.Jpeg.JpegDirectory>().FirstOrDefault();
            if (jpegDirectory != null)
            {
                width = jpegDirectory.GetImageWidth();
                height = jpegDirectory.GetImageHeight();
            }

            // Extract EXIF data
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSubIfdDirectory != null)
            {
                if (exifSubIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var date))
                {
                    dateTaken = date.ToUniversalTime();
                }
            }

            // Extract GPS location
            //var gpsDirectory = directories.OfType<MetadataExtractor.Formats.Exif.GpsDirectory>().FirstOrDefault();
            //if (gpsDirectory != null)
            //{
            //    if (gpsDirectory?.TryGetGeoLocation() is GeoLocation geoLocation)
            //    {
            //        Console.WriteLine($"Lat: {geoLocation.Latitude}, Long: {geoLocation.Longitude}");
            //        location = $"{geoLocation.Latitude:F6}, {geoLocation.Longitude:F6}";
            //    }
            //}

            // Extract orientation
            var exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (exifIfd0Directory != null && exifIfd0Directory.TryGetInt32(ExifDirectoryBase.TagOrientation, out var orient))
            {
                orientation = orient;
            }

            _logger.LogInformation("Metadata extracted: {Width}x{Height}, DateTaken: {DateTaken}",
                width, height, dateTaken);

            return (width, height, dateTaken, location, orientation);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract metadata, using defaults");
            return (0, 0, null, null, 1);
        }
    }
}
