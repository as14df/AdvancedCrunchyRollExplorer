using AdvancedCrunchyRollExplorer.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AdvancedCrunchyRollExplorer;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    public ObservableCollection<Anime> Animes { get; set; } = [];
    public ObservableCollection<Anime> FilteredAnimes { get; set; } = [];

    public List<string> SortColumns { get; } = ["Title", "Description", "Genres", "Episodes", "Seasons", "Year"];
    public List<string> SortOrders { get; } = new() { "Ascending", "Descending" };

    private string _selectedSortColumn = "Title";
    public string SelectedSortColumn
    {
        get => _selectedSortColumn;
        set
        {
            if (_selectedSortColumn != value)
            {
                _selectedSortColumn = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }
    }

    private string _selectedSortOrder = "Ascending";
    public string SelectedSortOrder
    {
        get => _selectedSortOrder;
        set
        {
            if (_selectedSortOrder != value)
            {
                _selectedSortOrder = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadData();
        ApplyFilterAndSort();
    }

    private void LoadData()
    {
        try
        {
            string json = null;
            string filePath = "animes.json";

            if (!File.Exists(filePath))
            {
                using var stream = FileSystem.OpenAppPackageFileAsync(filePath).Result;
                using var reader = new StreamReader(stream);
                json = reader.ReadToEnd();
            }
            else
            {
                json = File.ReadAllText(filePath);
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<List<Anime>>(json, options);

            if (items != null)
                foreach (var item in items)
                    Animes.Add(item);
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load JSON: {ex.Message}", "OK");
        }
    }

    private void ApplyFilterAndSort()
    {
        IEnumerable<Anime> query = Animes;

        // Filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            string lower = SearchText.ToLower();
            query = query.Where(a =>
                (a.Title?.ToLower().Contains(lower) ?? false) ||
                (a.Description?.ToLower().Contains(lower) ?? false) ||
                (a.GenresDisplay?.ToLower().Contains(lower) ?? false));
        }

        // Sort
        bool desc = SelectedSortOrder == "Descending";
        query = SelectedSortColumn switch
        {
            "Title" => desc ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "Description" => desc ? query.OrderByDescending(a => a.Description) : query.OrderBy(a => a.Description),
            "Genres" => desc ? query.OrderByDescending(a => a.GenresDisplay) : query.OrderBy(a => a.GenresDisplay),
            "Episodes" => desc ? query.OrderByDescending(a => a.Episodes) : query.OrderBy(a => a.Episodes),
            "Seasons" => desc ? query.OrderByDescending(a => a.Seasons) : query.OrderBy(a => a.Seasons),
            "Year" => desc ? query.OrderByDescending(a => a.Publish_Date) : query.OrderBy(a => a.Publish_Date),
            _ => query
        };

        // Refresh collection
        FilteredAnimes.Clear();
        foreach (var a in query)
            FilteredAnimes.Add(a);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SearchText = e.NewTextValue;
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
