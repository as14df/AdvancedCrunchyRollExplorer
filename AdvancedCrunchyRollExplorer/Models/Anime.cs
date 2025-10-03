namespace AdvancedCrunchyRollExplorer.Models;

public class Anime
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<string> Ratings { get; set; }
    public int? Episodes { get; set; }
    public int? Seasons { get; set; }
    public List<string> Genres { get; set; }
    public List<string> Descriptors { get; set; }
    public int? Publish_Date { get; set; }
    public string Image_Base64 { get; set; }

    // Computed property to display the base64 image
    public ImageSource ImageSource
    {
        get
        {
            if (string.IsNullOrEmpty(Image_Base64))
                return null;

            byte[] bytes = Convert.FromBase64String(Image_Base64);
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }

    public string RatingsDisplay => Ratings != null ? string.Join(", ", Ratings) : "";
    public string GenresDisplay => Genres != null ? string.Join(", ", Genres) : "";
    public string DescriptorsDisplay => Descriptors != null ? string.Join(", ", Descriptors) : "";
}