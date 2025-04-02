using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
namespace MoviesDataCleaning;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    #region Private Members

    private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    //private string _jsonPath = "./movie-list.json";

    private string _apiKey = "";
    private int _batchSize = 10;
    private int _startLine = 2;
    private int _endLine = 100;

    private string _inputPath = "";
    private string _outputPath = "";

    #endregion

    #region Constructor

    public MainWindow()
    {
        InitializeComponent();

        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        _apiKey = config.AppSettings.Settings[nameof(_apiKey)].Value;
        _startLine = int.Parse(config.AppSettings.Settings[nameof(_startLine)].Value);
        _endLine = int.Parse(config.AppSettings.Settings[nameof(_endLine)].Value);
        _batchSize = int.Parse(config.AppSettings.Settings[nameof(_batchSize)].Value);
        _inputPath = config.AppSettings.Settings[nameof(_inputPath)].Value;
        _outputPath = config.AppSettings.Settings[nameof(_outputPath)].Value;
        c_apiKey.Text = _apiKey;
        c_FilePath.Text = _inputPath;
        c_OutFilePath.Text = _outputPath;
        c_batchSize.Text = _batchSize.ToString();
        c_startLine.Text = _startLine.ToString();
        c_endLine.Text = _endLine.ToString();
    }

    #endregion

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
    }

    #region Event Handlers

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ProcessEnglishMovies();
    }
    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        ProcessJapaneseMovies();
    }

    private void c_apiKey_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _apiKey = (sender as TextBox)?.Text ?? "";
        SaveConfig(nameof(_apiKey), _apiKey);
    }


    private void c_startLine_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        try
        {
            _startLine = int.Parse((sender as TextBox)?.Text ?? "");
            SaveConfig(nameof(_startLine), _startLine.ToString());
        }
        catch
        {
            MessageBox.Show("Failed to parse start line");
        }
    }

    private void c_endLine_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

        try
        {
            _endLine = int.Parse((sender as TextBox)?.Text ?? "");
            SaveConfig(nameof(_endLine), _endLine.ToString());
        }
        catch
        {
            MessageBox.Show("Failed to parse end line");
        }
    }

    private void c_batchSize_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {

        try
        {
            _batchSize = int.Parse((sender as TextBox)?.Text ?? "");
            SaveConfig(nameof(_batchSize), _batchSize.ToString());
        }
        catch
        {
            MessageBox.Show("Failed to parse end line");
        }
    }


    private void c_outputFile_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _outputPath = (sender as TextBox)?.Text ?? "";
        SaveConfig(nameof(_outputPath), _outputPath);
    }


    private void c_inputFile_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _inputPath = (sender as TextBox)?.Text ?? "";
        SaveConfig(nameof(_inputPath), _inputPath);
    }

    #endregion

    #region Private Methods
    private void ProcessJapaneseMovies()
    {
        Task.Run(() =>
        {
            try
            {
                ClearLog();
                Log("Reading movies from file...");
                var list = LoadJapaneseDataFromFile(_inputPath, _startLine, _endLine);

                Log("Read movies successfully");

                Log("Sending api requests to ai agent for data standardization");

                for(int i = 0; i < list.Count; i += _batchSize)
                {
                    var entries = list?.Skip(i).Take(_batchSize).ToList() ?? [];
                    GetAiTitleAsync(entries).Wait();
                    SortGenericOrTitleAsync(entries).Wait();
                    GetPlatformAsync(entries).Wait();
                }
                Log("Finished data standardization");

                Log("Saving to output file");
                WriteDataToFile(_outputPath, _startLine, _endLine, list);
                Log("Finished saving to output file");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Log("Error: " + ex.Message);
            }
        });

    }

    private void ProcessEnglishMovies()
    {
        Task.Run(() =>
        {
            try
            {
                ClearLog();
                Log("Reading movies from file...");
                var list = LoadDataFromFile(_inputPath, _startLine, _endLine);

                Log("Read movies successfully");

                Log("Sending api requests to ai agent for data standardization");

                for(int i = 0; i < list.Count; i += _batchSize)
                {
                    var entries = list?.Skip(i).Take(_batchSize).ToList() ?? [];
                    GetAiTitleAsync(entries).Wait();
                    SortGenericOrTitleAsync(entries).Wait();
                    GetPlatformAsync(entries).Wait();
                }
                Log("Finished data standardization");

                Log("Saving to output file");
                WriteDataToFile(_outputPath, _startLine, _endLine, list);
                Log("Finished saving to output file");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                Log("Error: " + ex.Message);
            }
        });

        // Read the content of the file
        // Go line by line
        // For each line
        // Give ai the country of the line and the title, and ask it if the title is in english or not
        // And if it is not english ask it to translate it (return 1 if it is english with the title having spell errors fixed, and translated if not english)
        // Take that title, do some regex comparison with a movies database to get the title that matches.
        // use that to get the correct title and the platforms that the movie is part of

    }

    //private void ShowStats(List<MovieEntry>? entries = null)
    //{
    //    var list = entries ?? JsonSerializer.Deserialize<List<MovieEntry>>(File.ReadAllText(_jsonPath)) ?? [];
    //    list = list.Take(_sampleSize).ToList();

    //    var total = 0;
    //    var correctTitlesByDefault = 0;
    //    var correctIgnoringCase = 0;
    //    var correctWithoutPunctuation = 0;
    //    var aiCorrect = 0;
    //    var aiCorrectIgnoreCase = 0;
    //    var aiGenericCorrect = 0;

    //    var confidence100Total = 0;
    //    var confidence100Correct = 0;
    //    var confidence100CorrectIgnoringCase = 0;


    //    foreach(var entry in list)
    //    {
    //        if(entry.Ai_Confidence == 100)
    //        {
    //            confidence100Total += 1;
    //            confidence100Correct += entry.C_AiEqual ? 1 : 0;
    //            confidence100CorrectIgnoringCase += entry.C_AiEqualIgnoringCase ? 1 : 0;
    //        }
    //        total++;
    //        correctTitlesByDefault += entry.C_TitlesEqual ? 1 : 0;
    //        correctIgnoringCase += entry.C_TitlesEqualIgnoringCase ? 1 : 0;
    //        correctWithoutPunctuation += entry.C_TitlesEqualIgnoringPunctuationAndSpace ? 1 : 0;
    //        aiCorrect += entry.C_AiEqual ? 1 : 0;
    //        aiCorrectIgnoreCase += entry.C_AiEqualIgnoringCase ? 1 : 0;
    //        aiGenericCorrect += entry.Ai_IsGenericTermAndNotTitle == entry.O_IsGenericTermAndNotTitle ? 1 : 0;
    //    }


    //    Log($"""
    //            ==============================================================================================================
    //            total: {total}
    //            correct =>   {correctTitlesByDefault}
    //            correct % => {correctTitlesByDefault / (double)total * 100}

    //            ignoring case =>   {correctIgnoringCase}
    //            ignoring case % => {correctIgnoringCase / (double)total * 100}

    //            ignoring space & punctuation =>    {correctWithoutPunctuation}
    //            ignoring space & punctuation %  => {correctWithoutPunctuation / (double)total * 100}

    //            using ai title =>    {aiCorrect}
    //            using ai title %  => {aiCorrect / (double)total * 100}

    //            using ai title ignoring case =>    {aiCorrectIgnoreCase}
    //            using ai title ignoring case %  => {aiCorrectIgnoreCase / (double)total * 100}

    //            using ai title with 100 confidence =>    {confidence100Correct}
    //            using ai title with 100 confidence %  => {confidence100Correct / (double)confidence100Total * 100}

    //            using ai title ignoring case with 100 confidence =>    {confidence100CorrectIgnoringCase}
    //            using ai title ignoring case with 100 confidence %  => {confidence100CorrectIgnoringCase / (double)confidence100Total * 100}

    //            using ai is generic title =>    {aiGenericCorrect}
    //            using ai is generic title %  => {aiGenericCorrect / (double)total * 100}

    //            """);

    //}

    //private void ShowFirstEntries(List<MovieEntry>? entries = null)
    //{
    //    var list = entries ?? JsonSerializer.Deserialize<List<MovieEntry>>(File.ReadAllText(_jsonPath)) ?? [];
    //    list = list.Take(_sampleSize).ToList();

    //    var output = "Title --> AI Title --> Cleaned Title --> Confidence" + Environment.NewLine;
    //    foreach(var entry in list)
    //    {
    //        output += entry.Title + " -- " + entry.Ai_Title + " -- " + entry.O_CleanedTitle + " -- " + entry.Ai_Confidence + Environment.NewLine;
    //    }

    //    ClearLog();
    //    Log(output);
    //}

    private void WriteDataToFile(string outputPath, int startLine, int endLine, List<MovieEntry> entries)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet();

        for(int i = 0; i < entries.Count; i++)
        {
            var row = worksheet.Row(startLine + i);
            var entry = entries[i];

            row.Cell("A").SetValue(entry.PanelisId);
            row.Cell("B").SetValue(entry.Batch);
            row.Cell("C").SetValue(entry.Market);
            row.Cell("D").SetValue(entry.Gender);
            row.Cell("E").SetValue(entry.Age);
            row.Cell("F").SetValue(entry.Region);
            row.Cell("G").SetValue(entry.Access);
            row.Cell("H").SetValue(entry.Usage);
            row.Cell("I").SetValue(entry.Day);
            row.Cell("J").SetValue(entry.Date);
            row.Cell("K").SetValue(entry.Content);
            row.Cell("L").SetValue(entry.Title);
            row.Cell("M").SetValue(entry.Platform);
            row.Cell("N").SetValue(entry.EpisodeName);
            row.Cell("O").SetValue(entry.Season);
            row.Cell("P").SetValue(entry.ViewTime);
            row.Cell("Q").SetValue(entry.DurationViewed);
            row.Cell("R").SetValue(entry.DeviceViewed);
            row.Cell("S").SetValue(entry.Ai_Platform);
            row.Cell("T").SetValue(entry.Ai_Title);
            row.Cell("U").SetValue(entry.Ai_IsGenericTermAndNotTitle ? "generic" : "title");
            row.Cell("V").SetValue(entry.O_AllocatedTitle);

        }

        workbook.SaveAs(outputPath);
    }
    private List<MovieEntry> LoadJapaneseDataFromFile(string filePath, int startLine, int endLine)
    {
        var list = new List<MovieEntry>();

        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheets.LastOrDefault();

        if(worksheet is null)
        {
            MessageBox.Show("ERROR: no worksheet found in file");
            return [];
        }

        foreach(var row in worksheet.Rows(startLine, endLine))
        {
            var entry = new MovieEntry()
            {
                Market = row.Cell("A").GetStr(),
                PanelisId = row.Cell("B").GetStr(),
                ViewTime = row.Cell("D").GetStr(),
                Title = row.Cell("E").GetStr(),
                Season = row.Cell("F").GetStr(),
                EpisodeName = row.Cell("G").GetStr(),
                Platform = row.Cell("H").GetStr(),
                DurationViewed = row.Cell("I").GetStr(),
                DeviceViewed = row.Cell("J").GetStr(),
            };

            if(row.Cell("C").Value.IsBlank == false)
            {
                if(DateTime.TryParseExact(row.Cell("C").GetStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
                    entry.Date = date;
            }
            list.Add(entry);
        }
        return list;
    }

    private List<MovieEntry> LoadDataFromFile(string filePath, int startLine, int endLine)
    {
        var list = new List<MovieEntry>();

        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheets.LastOrDefault();

        if(worksheet is null)
        {
            MessageBox.Show("ERROR: no worksheet found in file");
            return [];
        }

        foreach(var row in worksheet.Rows(startLine, endLine))
        {
            var entry = new MovieEntry()
            {
                PanelisId = row.Cell("A").GetStr(),
                Batch = row.Cell("B").GetStr(),
                Market = row.Cell("C").GetStr(),
                Gender = row.Cell("D").GetStr(),
                Age = row.Cell("E").GetInt(),
                Region = row.Cell("F").GetStr(),
                Access = row.Cell("G").GetStr(),
                Usage = row.Cell("H").GetStr(),
                Day = row.Cell("I").GetStr(),
                Content = row.Cell("K").GetStr(),
                Title = row.Cell("L").GetStr(),
                Platform = row.Cell("M").GetStr(),
                EpisodeName = row.Cell("N").GetStr(),
                Season = row.Cell("O").GetStr(),
                ViewTime = row.Cell("P").GetStr(),
                DurationViewed = row.Cell("Q").GetStr(),
                DeviceViewed = row.Cell("R").GetStr(),
                O_CleanedPlatform = row.Cell("S").GetStr(),
                O_CleanedTitle = row.Cell("T").GetStr(),
                O_IsGenericTermAndNotTitle = row.Cell("U").GetStr()?.ToLower() != "title",
                O_AllocatedTitle = row.Cell("V").GetStr(),
            };

            if(row.Cell("J").Value.IsBlank == false)
            {
                var value = ((int)row.Cell("J").Value.GetNumber()).ToString();
                if(DateTime.TryParseExact(value, "YYYYMMDD", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
                    entry.Date = date;
            }
            list.Add(entry);
        }

        return list;
    }

    private async Task GetAiTitleAsync(List<MovieEntry> entries)
    {
        var prompt = $$"""
            You are an expert movie or series titles extracting system. Your task is to take as input a movie or a series title and the year it was watched and the country which the user has watched it in.
            And you will return the correct title of the movie or series.
            In case the title has misspellings or is in a different language other than English fix the misspellings and translate it into English except when it is in Japanese do not translate it and keep it in Japanese.
            Return the standard name of the movie or series as it is present on platforms like Netflix.
            Take into consideration the popularity of the movie at the time it was watched when determining which title to choose.
            If it is a movie try to return the most recent release and if it is a series try to return the latest season.
            If it is a sports tournament return the full name of the tournament.
            If the title is something that is generic or contains gibberish such as: can't remember or basketball or anything or korean movie then return a generic title representing it and make the first letter uppercase.
            If the title contains multiple phrases separated by a comma (,) only take into consideration the first phrase into determining the movie name.
            If the title is not a title of a movie or series or sport event that exists return N/A as title name. including the confidence at the start.

            Here is a list of generic terms and their mappings to use when the title is a generic term:
            [
                Anime or Anime's or Animes or อนิเมะ มายฮีโร่อะคาเดมี่ or นารูโตะ or ยอดนักสืบจิ๋วโคนัน or การ์ตูนอนิเมะ or การ์ตูนผจญภัย or Anime => Anime
                K-drama series or K-series or K-Drama or Korean Drama series or Korean drama or K dramas or Kdara or dramakorea or Drakor or Serial drakor or ซีรีส์เกาหลี or เกาหลี => Korean Drama
                Chinese drama or Chinese or Drama china or Drama fantasi cina or ซีรีย์จีน or ซีรีส์จีน or อนิเมะจีน or หนังจีน จำชื่อไม่ได้ or ซีรีย์ จีน => Chinese Drama
                Hindi series or Hindi TV Series => Hindi
                India seriese or India movie => India
                Tamil series or Tamil TV Series => Tamil
                GL seriese or GL => Thai GL Series
                BL or boyslove or BL seriese => Thai BL Series
                Thai movie => Thai Movie
                Hindi movies or Hindi movie => Hindi
                Tamil movies or Tamil movie => Tamil Movie
                Indonesian Horror or Indo horror => Indonesian Horror
                Indonesian Drama or Indo drama => Indonesian Drama
                bollywood or Bollywood movies or Bollywood movie => Bollywood Movies
                hollywood or hollywood movies or hollywood movie => Hollywood Movies
                blockbuster or blogbuster or blockbuster movie => BLOGBUSTER movie
                Horror or Horror Movies or horror => Horror
                Drama => Drama
                football or ฟุตบอล or ฟุตบอล => Football
                bola or soccer or sepak bola or Pertandingan bola => Football
                Volleyball or volleyball => Volleyball
                badminton or badminthon => Badminton
                bascketball => Basketball
            ]

            You will get as input a list of movies and you will return a list of movie titles separated by new lines and in each line, start with a number between 0 and 100 representing how confident are you in the title you have returned.

            Here is movies or series details list:
            '''
            [
            {{string.Join(Environment.NewLine, entries.Select(entry =>
                    {
                        //- Region watched from: {{entry.Region}}
                        return
                        $$"""
                        {
                            - Title: {{entry.Title}}
                            - Type of content: {{entry.Content}}
                            - Country watched from: {{entry.Market}}
                        }
                    
                        """;
                    }))}}
            ]
            '''

            The output that you should return is a string representing the titles of the movies or series without any other text around it.
            Do not put any other text in your answer, only the titles separated by a new line and each line will start by a number between 0 and 100 representing how confident you are in the returned title followed by a space then the title.
            Return the titles in the order that the input elements are in.
            """;


        var client = new HttpClient();

        var request = $$"""
            {
              "contents": [
                {
                  "parts": [{ "text": "{{prompt}}"}]
                }
              ]
            }
            """;

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var content = new StringContent(request, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);
        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            MessageBox.Show("Too many requests have been sent, please try again in a bit");
            return;
        }

        var result = JsonSerializer.Deserialize<ApiResponse>(response.Content.ReadAsStream(), options: _jsonSerializerOptions);


        var titlesStr = result?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text.Trim() ?? "";

        var titles = titlesStr.Split("\n");

        for(int i = 0; i < entries.Count; i++)
        {
            var element = titles.ElementAt(i);
            var number = element.Split(" ").First();
            var title = element[number.Length..];

            entries[i].Ai_Confidence = double.Parse(number);
            entries[i].Ai_Title = title.Trim();
        }
    }

    private async Task SortGenericOrTitleAsync(List<MovieEntry> entries)
    {

        var prompt = $$"""
            You are an expert movie and series and tournaments titles classification system. Your task is to take as input a movie or a series title and you will return one of two values: title or generic. 
            title: which will represent that the title is a correct movie or series title or sports tournament name that exists.
            generic: which will represent a generic term like: drama or movie, which will not be the title of an existing movie or series or sports tournament. 
            
            Here is a list of some generic terms that could exist:
            [
            
                Anime or Anime's or Animes or อนิเมะ มายฮีโร่อะคาเดมี่ or นารูโตะ or ยอดนักสืบจิ๋วโคนัน or การ์ตูนอนิเมะ or การ์ตูนผจญภัย or Anime
                K-drama series or K-series or K-Drama or Korean Drama series or Korean drama or K dramas or Kdara or dramakorea or Drakor or Serial drakor or ซีรีส์เกาหลี or เกาหลี Korean Drama
                Chinese drama or Chinese or Drama china or Drama fantasi cina or ซีรีย์จีน or ซีรีส์จีน or อนิเมะจีน or หนังจีน จำชื่อไม่ได้ or ซีรีย์ จีน
                Hindi series or Hindi TV Series or Hindi
                India seriese or India movie or India
                Tamil series or Tamil TV Series or Tamil
                GL seriese or GL Thai
                BL or boyslove or BL seriese or Thai BL Series
                Thai movie 
                Hindi movies or Hindi movie or Hindi
                Tamil movies or Tamil movie or Tamil Movie
                Indonesian Horror or Indo horror
                Indonesian Drama or Indo drama
                bollywood or Bollywood movies or Bollywood movie
                hollywood or hollywood movies or hollywood movie 
                blockbuster or blogbuster or blockbuster movie 
                Horror or Horror Movies or horror
                Drama 
                football or ฟุตบอล or ฟุตบอล
                bola or soccer or sepak bola or Pertandingan bola
                Volleyball or volleyball
                badminton or badminthon
                bascketball or Basketball
            ]

            You will get as input a list of movies titles and you will return a list of words, which will be either generic or title
            
            Here is movies or series list:
            '''
            [
            {{string.Join(Environment.NewLine, entries.Select(entry => entry.Ai_Title))}}
            ]
            '''

            Return a line in the output for each line in the input, do not miss any even if the title is repeated.
            If the title is equal to N/A return generic.
            
            The output should not contain any other text around it. It should only contain the generic or title result. 
            Return a json array of strings which will represent the output for each line.
            Return the words in the order that the input elements are in.
            The result should have the same number of line as the number of titles provided.
            """;


        var client = new HttpClient();

        var request = $$"""
            {
              "contents": [
                {
                  "parts": [{ "text": "{{prompt}}"}]
                }
              ]
            }
            """;

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var content = new StringContent(request, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            MessageBox.Show("Too many requests have been sent, please try again in a bit");
            return;
        }

        var result = JsonSerializer.Deserialize<ApiResponse>(response.Content.ReadAsStream(), options: _jsonSerializerOptions);


        var wordsStr = result?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text.Trim() ?? "";

        var words = wordsStr.Split("\n");

        for(int i = 0; i < entries.Count; i++)
        {
            entries[i].Ai_IsGenericTermAndNotTitle = words.ElementAt(i).Trim() == "generic";
        }
    }

    private async Task GetPlatformAsync(List<MovieEntry> entries)
    {
        var prompt = $$"""
            You are an expert movie and series and tournaments titles to platform scraping system. Your task is to take as input a movie or a series title and a list of presumed platforms that they exist in and you will return one platform that it is streamed on.
            Make sure to return the correct value that exists on the web. If none of the platforms stream the movie return another platform name which is not part of the list that you are sure it is streamed on.
            You will take into consideration the country that he movie has been watched on and the time it was watched for availability.

            You will get as input a list of movies details and you will return a list of platforms.
            
            Here is movies or series list:
            '''
            {{string.Join(Environment.NewLine, entries.Select(entry =>
                {
                    return $$"""
                        {
                            - Title: {{entry.Title}}
                            - Type of content: {{entry.Content}}
                            - Country watched from: {{entry.Market}}
                            - Presumed Platforms: {{entry.Platform}}
                        }

                        """;
                }))}}
            ]

            Return a line in the output for each element in the input, do not miss any even if the line is repeated or empty.
            
            The output should not contain any other text around it. It should only contain the platforms separated by a new line. 
            The result should have the same number of line as the number of elements provided.
            """;


        var client = new HttpClient();

        var request = $$"""
            {
              "contents": [
                {
                  "parts": [{ "text": "{{prompt}}"}]
                }
              ]
            }
            """;

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var content = new StringContent(request, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            MessageBox.Show("Too many requests have been sent, please try again in a bit");
            return;
        }

        var result = JsonSerializer.Deserialize<ApiResponse>(response.Content.ReadAsStream(), options: _jsonSerializerOptions);


        var wordsStr = result?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text.Trim() ?? "";

        var words = wordsStr.Split("\n");

        for(int i = 0; i < entries.Count; i++)
        {
            entries[i].Ai_Platform = words.ElementAt(i);
        }
    }

    #endregion

    #region Helpers

    private void Log(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            c_logs.Text += message + Environment.NewLine;
        });
    }

    private void ClearLog()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            c_logs.Text = "";
        });
    }

    private int CalcLevenshteinDistance(string a, string b)
    {
        if(string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
        {
            return 0;
        }

        if(string.IsNullOrEmpty(a))
        {
            return b.Length;
        }

        if(string.IsNullOrEmpty(b))
        {
            return a.Length;
        }

        int lengthA = a.Length;
        int lengthB = b.Length;
        var distances = new int[lengthA + 1, lengthB + 1];

        for(int i = 0; i <= lengthA; distances[i, 0] = i++) ;
        for(int j = 0; j <= lengthB; distances[0, j] = j++) ;

        for(int i = 1; i <= lengthA; i++)
        {
            for(int j = 1; j <= lengthB; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;

                distances[i, j] = Math.Min(
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost
                );
            }
        }

        return distances[lengthA, lengthB];
    }

    private void SaveConfig(string key, string value)
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings[key].Value = value;
        config.Save(ConfigurationSaveMode.Modified);
    }

    #endregion

}
