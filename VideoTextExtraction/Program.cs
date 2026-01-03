using System.CommandLine;
using Xabe.FFmpeg;
using VideoTextExtraction.Models;
using VideoTextExtraction.Services;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Set FFmpeg path (user needs to have FFmpeg installed)
        // You can also set this via environment variable: XABE_FFMPEG_PATH
        // FFmpeg.SetExecutablesPath("/path/to/ffmpeg");

        PrintBanner();

        // Create root command
        var rootCommand = new RootCommand("Extract text from videos using subtitles or OCR");

        // Input video option
        var videoOption = new Option<FileInfo>(
            aliases: new[] { "--video", "-v" },
            description: "Path to the video file")
        {
            IsRequired = true
        };
        videoOption.AddValidator(result =>
        {
            var file = result.GetValueForOption(videoOption);
            if (file != null && !file.Exists)
            {
                result.ErrorMessage = $"Video file not found: {file.FullName}";
            }
        });

        // Output file option
        var outputOption = new Option<FileInfo>(
            aliases: new[] { "--output", "-o" },
            description: "Path to the output text file")
        {
            IsRequired = true
        };

        // FPS option for OCR
        var fpsOption = new Option<int>(
            aliases: new[] { "--fps", "-f" },
            description: "Frames per second to extract for OCR (default: 1)",
            getDefaultValue: () => 1);

        // Force OCR option
        var forceOcrOption = new Option<bool>(
            aliases: new[] { "--force-ocr" },
            description: "Force OCR extraction even if subtitles are available",
            getDefaultValue: () => false);

        // Include timestamps option
        var timestampsOption = new Option<bool>(
            aliases: new[] { "--timestamps", "-t" },
            description: "Include timestamps in output",
            getDefaultValue: () => false);

        // Language option
        var languageOption = new Option<string?>(
            aliases: new[] { "--language", "-l" },
            description: "Preferred subtitle language (e.g., 'eng', 'spa')");

        // OCR language option
        var ocrLangOption = new Option<string>(
            aliases: new[] { "--ocr-lang" },
            description: "OCR language for Tesseract (default: eng)",
            getDefaultValue: () => "eng");

        // TessData path option
        var tessDataOption = new Option<string>(
            aliases: new[] { "--tessdata" },
            description: "Path to Tesseract data directory",
            getDefaultValue: () => "./tessdata");

        // Remove duplicates option
        var removeDuplicatesOption = new Option<bool>(
            aliases: new[] { "--remove-duplicates" },
            description: "Remove duplicate text from consecutive frames",
            getDefaultValue: () => true);

        // Video length limit option
        var videoLengthOption = new Option<int>(
            aliases: new[] { "--video-length" },
            description: "Maximum video length to process in minutes (0 = entire video)",
            getDefaultValue: () => 0);

        // Parallel processing option
        var parallelProcessingOption = new Option<bool>(
            aliases: new[] { "--parallel" },
            description: "Enable parallel processing for faster OCR extraction",
            getDefaultValue: () => false);

        // Max degree of parallelism option
        var maxParallelismOption = new Option<int>(
            aliases: new[] { "--threads" },
            description: "Maximum number of parallel threads (0 = auto-detect CPU cores)",
            getDefaultValue: () => 0);

        rootCommand.AddOption(videoOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(fpsOption);
        rootCommand.AddOption(forceOcrOption);
        rootCommand.AddOption(timestampsOption);
        rootCommand.AddOption(languageOption);
        rootCommand.AddOption(ocrLangOption);
        rootCommand.AddOption(tessDataOption);
        rootCommand.AddOption(removeDuplicatesOption);
        rootCommand.AddOption(videoLengthOption);
        rootCommand.AddOption(parallelProcessingOption);
        rootCommand.AddOption(maxParallelismOption);

        // Use custom handler to avoid parameter limit (max 8 in SetHandler)
        rootCommand.SetHandler(async (context) =>
        {
            var video = context.ParseResult.GetValueForOption(videoOption);
            var output = context.ParseResult.GetValueForOption(outputOption);
            var fps = context.ParseResult.GetValueForOption(fpsOption);
            var forceOcr = context.ParseResult.GetValueForOption(forceOcrOption);
            var timestamps = context.ParseResult.GetValueForOption(timestampsOption);
            var language = context.ParseResult.GetValueForOption(languageOption);
            var ocrLang = context.ParseResult.GetValueForOption(ocrLangOption);
            var tessData = context.ParseResult.GetValueForOption(tessDataOption);
            var removeDuplicates = context.ParseResult.GetValueForOption(removeDuplicatesOption);
            var videoLength = context.ParseResult.GetValueForOption(videoLengthOption);
            var enableParallel = context.ParseResult.GetValueForOption(parallelProcessingOption);
            var maxParallelism = context.ParseResult.GetValueForOption(maxParallelismOption);

            if (video == null || output == null)
            {
                Logger.Error("Video and output options are required");
                context.ExitCode = 1;
                return;
            }

            var options = new ProcessingOptions
            {
                VideoPath = video.FullName,
                OutputPath = output.FullName,
                FramesPerSecond = fps,
                ForceOcr = forceOcr,
                IncludeTimestamps = timestamps,
                PreferredLanguage = language,
                OcrLanguage = ocrLang,
                TessDataPath = tessData,
                RemoveDuplicates = removeDuplicates,
                MaxVideoLengthMinutes = videoLength,
                EnableParallelProcessing = enableParallel,
                MaxDegreeOfParallelism = maxParallelism
            };

            await ProcessVideoAsync(options);
        });

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ProcessVideoAsync(ProcessingOptions options)
    {
        try
        {
            var extractor = new VideoTextExtractor();
            var result = await extractor.ExtractTextAsync(options);

            if (result.Success)
            {
                Console.WriteLine();
                Logger.Success("=== Extraction Summary ===");
                Logger.Info($"Method: {result.Method}");
                Logger.Info($"Segments: {result.SegmentCount}");
                Logger.Info($"Processing Time: {result.ProcessingTimeSeconds:F2} seconds");
                Logger.Info($"Output File: {result.OutputPath}");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine();
                Logger.Error("=== Extraction Failed ===");
                Logger.Error(result.ErrorMessage ?? "Unknown error");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Fatal error", ex);
            Environment.Exit(1);
        }
    }

    static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║        VIDEO TEXT EXTRACTION TOOL                       ║
║        Extract text from videos via subtitles or OCR    ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }
}
