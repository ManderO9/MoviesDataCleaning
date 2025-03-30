using ClosedXML.Excel;
using System.IO;
using System.Windows;
namespace MoviesDataCleaning;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Constructor

    public MainWindow()
    {
        InitializeComponent();
    }

    #endregion

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        c_FilePath.Text = @"C:\Users\nnnn\Desktop\data cleaning\J2025ONDEVICE001_DIARY_PROJECT_WEEK1_2025317_121855 - Example.xlsx";
    }

    #region Event Handlers

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        LoadMoviesDatabase();
    }

    #endregion


    #region Private Helpers

    private void LoadMoviesDatabase()
    {
        // TODO: go through all the code and everywhere there could be and exception, try catch it and show a pupup when something
        // happens, and do a global try catch as well for all exceptions

        var path = c_FilePath.Text;
        var output = "";
        
        using(var workbook = new XLWorkbook(path))
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if(worksheet is null)
            {
                output = "worsheet is null";
                
            }
        }


        var stream = File.Open(path, FileMode.Open);

        var reader = new StreamReader(stream);
        var header = reader.ReadLine();

        output += header;

        output += Environment.NewLine + reader.ReadLine();

        c_logs.Text = output;


        // Read the content of the file
        // Go line by line
        // For each line
        // Give ai the country of the line and the title, and ask it if the title is in english or not
        // And if it is not english ask it to translate it (return 1 if it is english with the title having spell errors fixed, and translated if not english)
        // Take that title, do some regex comparison with a movies database to get the title that matches.
        // use that to get the correct title and the platforms that the movie is part of

    }
    
    #endregion

}
