using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WhatsAppReader.Model;
using LiveChartsCore.Defaults;

namespace WhatsAppReader;

public partial class MainPage : ContentPage
{
    // README
    //
    // STATUS BAR COLOR
    // Change status bar color ? Add the communitytoolkit.maui nugget
    // Then add it in the MauiProgram.cs :
    //public static MauiApp CreateMauiApp()
    //{
    //    var builder = MauiApp.CreateBuilder();
    //    builder
    //        .UseMauiApp<App>()
    //        // Initialize the .NET MAUI Community Toolkit by adding the below line of code
    //        .UseMauiCommunityToolkit()
    //        [...]
    //
    // Then add it to the MainPage.xaml : in the <ContentPage> tag, reference the xaml as mct :
    // xmlns:mct="clr-namespace:CommunityToolkit.Maui.Behaviors;assembly=CommunityToolkit.Maui"
    //
    // Then add the piece of code :
    // <ContentPage.Behaviors>
    //      <mct:StatusBarBehavior StatusBarColor = "pink" />
    // </ContentPage.Behaviors>

    // ORIENTATION
    // Orientation : in Plateforms / Android / MainActivity : Added ScreenOrientation = ScreenOrientation.Portrait

    // CHARTS
    // Add the LiveChartsCore.SkiaSharpView.Maui 2.0.0-beta.701 nugget (prerelease must be ticked)
    // Then add it in the MauiProgram.cs :
    // using SkiaSharp.Views.Maui.Controls.Hosting;
    // And :
    // public static MauiApp CreateMauiApp()
    // {
    //    var builder = MauiApp.CreateBuilder();
    //    builder
    //        .UseMauiApp<App>()
    //        // Initialize the .NET MAUI Skia Sharp by adding the below line of code
    //        .UseSkiaSharp(true)
    //        [...]
    //    
    // In the XAML, add the namespace : in the content tab
    // xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.Maui;assembly=LiveChartsCore.SkiaSharpView.Maui"
    //
    // And add the chart itself
    //

    // Data storage
    List<string> invalidLines = new List<string>();
    List<ChatLine> chatList = new List<ChatLine>();

    public MainPage()
	{
		InitializeComponent();
	}


    #region MENU
    // MENU ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load
    private void btnLoad_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible = true;
        slStats.IsVisible = false;

        btnLoad.Source = "load.png";
        btnStats.Source = "stats_disabled.png";

        bxLoad.Color = Color.FromArgb("#9a0089");
        bxStats.Color = Colors.Transparent;
    }

    // Stats
    private void btnStats_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible = false;
        slStats.IsVisible = true;

        btnLoad.Source = "load_disabled.png";
        btnStats.Source = "stats.png";

        bxLoad.Color = Colors.Transparent;
        bxStats.Color = Color.FromArgb("#9a0089");
    }
    #endregion

    #region LOAD
    // HELPERS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load the WhatsApp File
    private async void btnLoadFile_Clicked(object sender, EventArgs e)
	{
        // Open the file picker
        FilePickerFileType customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/*" } }, // MIME type
                { DevicePlatform.WinUI, new[] { ".txt", ".txt" } }, // file extension
            });

        PickOptions options = new()
        {
            PickerTitle = "Please select a csv file",
            FileTypes = customFileType,
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            invalidLines = new List<string>();
            chatList = new List<ChatLine>();

            // Do Stuff with the file
            Stream s = await result.OpenReadAsync();

            //using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("iso-8859-1")))
            using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("UTF-8")))
            {
                string currentLine;
                int realLines = 0;
                int keptLines = 0;

                // Read first line to skip it
                currentLine = sr.ReadLine();
                realLines++;
                
                while ((currentLine = sr.ReadLine()) != null)
                {
                    realLines++;
                    // RegEx :
                    // ^       : Start of the string
                    // \d{1,2} : Digit, 1 or 2 : the month
                    // /       : the / character
                    // \d{1,2} : Digit, 1 or 2 : the day
                    // /       : the / character
                    // \d{2}   : Exactly 2 digits : the year
                    // ,       : The coma then the space
                    // \d{2}   : Exactly 2 digits : the hours are HH
                    // :       : the : character
                    // \d{2}   : Exactly 2 digits : the minutes
                    //  -      : The next part
                    Regex rex = new Regex(@"^\d{1,2}/\d{1,2}/\d{2}, \d{2}:\d{2} - ");

                    // Input Validation
                    if (rex.IsMatch(currentLine))
                    {
                        keptLines++;

                        // Parse the Line :
                        // 5/29/22, 19:05 - Beb*: Va falloir négocier ça avec un bon verre de vin ou une bonne mousse (le mot élégant pour dire "bière", si mes manuels sont à jour) : je ne suis pas sur que 100% sobre, ce soit super intéressant 😅

                        // Date > M/d/YY, HH:mm
                        string datetimeString = currentLine.Split('-')[0];
                        DateTime dateTime = DateTime.ParseExact(datetimeString, "M/d/yy, HH:mm ", CultureInfo.InvariantCulture);

                        // Get the Author and message part, ignore the first space > Author: Message
                        currentLine = currentLine.Split('-')[1].TrimStart();

                        // Author is the first part of  : 
                        string Sender = currentLine.Split(':')[0];

                        // Message - +2 gets rid of the :_
                        string Message = currentLine.Substring(Sender.Length + 2);

                        ChatLine aChatLine = new ChatLine
                        {
                            Line = realLines,
                            DateTime = dateTime,
                            Sender = Sender,
                            Message = Message,
                            IsMedia = (Message == "<Media omitted>")
                        };

                        chatList.Add(aChatLine);
                    }
                    else
                    {
                        invalidLines.Add($"[{realLines}] : {currentLine}");
                        // Not valid. Must by a new line from a message : add it to the previous element in the List<>
                        if (chatList.Count > 1)
                        {
                            ChatLine lastChatLine = chatList[chatList.Count - 1];
                            lastChatLine.Message = $"{lastChatLine.Message}<br/>{currentLine}";
                        }
                    }
                }

                // Display results
                lblLogs.Text = $"found {realLines:n0} line{(realLines > 1 ? "s" : "")} / kept {keptLines:n0} valid";
                frmLogs.BackgroundColor = Color.FromArgb("7db497");
            }
        }

        // enable/disable the search button
        btnRandomLine.Source = (chatList.Count > 0 ? "randomline.png" : "randomline_disabled.png");
        btnRandomLine.IsEnabled = (chatList.Count > 0);
        btnSearchLine.IsEnabled = (chatList.Count > 0);
        btnSearchLine.Source = (chatList.Count > 0 ? "search.png" : "search_disabled.png");
        txtLineNumber.IsEnabled = (chatList.Count > 0);

        // Select a random line
        if (chatList.Count > 0)
            btnRandomLine_Clicked(null, null);

        // Refresh the Graphs
        UpdateCharts();
    }



    // Search for a random line
    private void btnRandomLine_Clicked(object sender, EventArgs e)
    {
        Random random = new Random();
        int index = random.Next(chatList.Count);
        ChatLine aChatLine = chatList[index];

        // Display results
        lblLineNumber.Text = $"file line : {aChatLine.Line.ToString()} / message number : {index + 1}";
        lblLineDateTime.Text = aChatLine.DateTime.ToString("dd MMM yyyy");
        lblLineSender.Text = aChatLine.Sender.ToString();
        lblLineMessage.Text = aChatLine.Message;
        lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");
    }

    // Search for a specific line
    private void btnSearchLine_Clicked(object sender, EventArgs e)
    {
        int theLine;
        if (int.TryParse(txtLineNumber.Text, out theLine))
        {
            if (theLine > 0)
            {
                if (theLine <= chatList.Count)
                {
                    ChatLine aChatLine = chatList[theLine-1];

                    // Display results
                    lblLineNumber.Text = $"file line : {aChatLine.Line.ToString()} / message number : {theLine}";
                    lblLineDateTime.Text = aChatLine.DateTime.ToString("dd MMM yyyy");
                    lblLineSender.Text = aChatLine.Sender.ToString();
                    lblLineMessage.Text = aChatLine.Message;
                    lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");

                    lblLogs.Text = $"Line [{theLine}] loaded.";
                    frmLogs.BackgroundColor = Color.FromArgb("7db497");
                }
                else
                {
                    // Not within reach
                    frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                    lblLogs.Text = $"Error : line number must be within line count : [1 -{chatList.Count}]";
                }
            }
            else
            {
                // Negative int
                frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                lblLogs.Text = "Error : line number must be positive.";
            }
        }
        else
        {
            // Not an int
            frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
            lblLogs.Text = "Error : line number must be a positive integer.";
        }
    }
    #endregion

    #region CHARTS
    // Main Charts Updater Routine
    private void UpdateCharts()
    {
        UpdateChartMessagesSent();
        UpdateChartMostMessagesSent();
        UpdateChartMessagesOverTime();
    }

    // Messages Sent
    private void UpdateChartMessagesSent()
    {
        // Calculate count of messages
        var groups = chatList.GroupBy(c => c.Sender)
                             .Select(c => new { Sender = c.Key, MessagesCnt = c.Count() });

        // Bar Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt },
            Name = groups.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse("#1c41ab")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#1c41ab")),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1c41ab"))
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(1).MessagesCnt },
            Name = groups.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse("#9a0089")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#9a0089")),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#9a0089"))
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt + groups.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse("#cccccc")) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse("#cccccc")),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#cccccc"))
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new List<ICartesianAxis>();

        // Axis
        Axis AxisX = new Axis { Labels = new string[] { "Sent" } };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtBarMessagesSent.Series = SeriesChart;
        crtBarMessagesSent.XAxes = Axis;
    }

    // Day with the most messages
    private void UpdateChartMostMessagesSent()
    {
        // Calculate count of messages
        /*
        var groups = chatList.GroupBy(c => new { c.DateTime.Date, c.Sender })
                             .Select(g => new { g.Key.Date, g.Key.Sender, MessagesCnt = g.Count() })
                             .OrderByDescending(x => x.MessagesCnt);
        */

        // Message Count by Date
        var chatLinesPerDate = chatList.GroupBy(c => new { c.DateTime.Date })
                                       .Select(g => new { g.Key.Date, MessagesCnt = g.Count() })
                                       .OrderByDescending(x => x.MessagesCnt);
        DateTime dateTimeMax = chatLinesPerDate.ElementAt(0).Date;

        var chatLinesForDateMax = chatList.Where(c => c.DateTime.Date == dateTimeMax)
                                          .GroupBy(c => new { c.Sender })
                                          .Select(g => new { g.Key.Sender, MessagesCnt = g.Count() })
                                          .OrderBy(x => x.Sender);

        // Bar Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse("#1c41ab")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#1c41ab")),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1c41ab"))
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse("#9a0089")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#9a0089")),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#9a0089"))
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt + chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse("#cccccc")) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse("#cccccc")),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#cccccc"))
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new List<ICartesianAxis>();

        // Axis
        Axis AxisX = new Axis { Labels = new string[] { dateTimeMax.ToString("dd MMM yyyy") } };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtBarMostMessagesSent.Series = SeriesChart;
        crtBarMostMessagesSent.XAxes = Axis;
    }

    // Messages Over time
    private void UpdateChartMessagesOverTime()
    {
        List<DateTimePoint> sender1 = new List<DateTimePoint>();
        List<DateTimePoint> sender2 = new List<DateTimePoint>();
        List<DateTimePoint> total = new List<DateTimePoint>();

        // Group by Date and Sender :
        // Date1, Sender1, 32
        // Date1, Sender2, 64
        // Date2, Sender1, 12
        // Date2, Sender2, 37
        // ...
        // Get Count by Month
        var chatLinesPerDate = chatList.GroupBy(c => new { DateYYYYMM = c.DateTime.ToString("yyyy/MM/01"), c.Sender })
                                       .Select(g => new { Date = g.Key.DateYYYYMM, g.Key.Sender, MessageCnt = g.Count() })
                                       .OrderBy(x => x.Date).ToList();

        // Get Distinct Sender names
        var senderNamesDistinct = chatLinesPerDate.Select(c => c.Sender).Distinct().ToList();

        // Get Distinct Dates
        var datesDistinct = chatLinesPerDate.Select(c => c.Date).Distinct().ToList();

        // Calculate the running totals
        int valSender1 = 0;
        int valSender2 = 0;

        foreach (var aDate in datesDistinct)
        {
            DateTime dateTime = DateTime.ParseExact(aDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            var aSender1 = chatLinesPerDate.FirstOrDefault(c => c.Date == aDate && c.Sender == senderNamesDistinct.ElementAt(0));
            var aSender2 = chatLinesPerDate.FirstOrDefault(c => c.Date == aDate && c.Sender == senderNamesDistinct.ElementAt(1));

            // Search for the value
            valSender1 += (aSender1 == null ? 0 : aSender1.MessageCnt);
            valSender2 += (aSender2 == null ? 0 : aSender2.MessageCnt);

            // Add
            sender1.Add(new DateTimePoint(dateTime, valSender1));
            sender2.Add(new DateTimePoint(dateTime, valSender2));
              total.Add(new DateTimePoint(dateTime, valSender1 + valSender2));
        }

        // Line Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Sender 1
        LineSeries<DateTimePoint> seriesSender1 = new LineSeries<DateTimePoint>
        {
            Values = sender1,
            Name = senderNamesDistinct.ElementAt(0),
            Stroke = new SolidColorPaint(SKColor.Parse("#1c41ab")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#551c41ab")),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#1c41ab")),
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#1c41ab")) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse("#1c41ab")),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender1);

        // Serie #2 : Sender 2
        LineSeries<DateTimePoint> seriesSender2 = new LineSeries<DateTimePoint>
        {
            Values = sender2,
            Name = senderNamesDistinct.ElementAt(1),
            Stroke = new SolidColorPaint(SKColor.Parse("#9a0089")) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse("#559a0089")),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#9a0089")),
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#9a0089")) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse("#9a0089")),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender2);

        // Serie #3 : Total
        LineSeries<DateTimePoint> seriesTotal = new LineSeries<DateTimePoint>
        {
            Values = total,
            Name = "Total",
            Stroke = new SolidColorPaint(SKColor.Parse("#cccccc")) { StrokeThickness = 2 },
            Fill = null,
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#cccccc")),
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#cccccc")) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse("#cccccc")),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesTotal);

        List<ICartesianAxis> Axis = new List<ICartesianAxis>();
        // Axis
        Axis AxisX = new Axis
        {
            // Set the label format as we want
            Labeler = value => new DateTime((long)value).ToString("yyyy MMM"),
            LabelsRotation = 30,
            // when using a date time type, let the library know your unit 
            // since all the months and years have a different number of days
            // we can use the average, it would not cause any visible error in the user interface
            // Months: TimeSpan.FromDays(30.4375).Ticks
            // Years: TimeSpan.FromDays(365.25).Ticks
            UnitWidth = TimeSpan.FromDays(30.4375).Ticks,
            MinStep = TimeSpan.FromDays(30.4375).Ticks
        };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtLineEvolution.Series = SeriesChart;
        crtLineEvolution.XAxes = Axis;
    }
    #endregion
}

