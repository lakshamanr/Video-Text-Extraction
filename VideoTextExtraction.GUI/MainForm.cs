using System;
using System.IO;
using System.Windows.Forms;
using VideoTextExtraction.Models;
using VideoTextExtraction.Services;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.GUI;

public partial class MainForm : Form
{
    private TextBox txtVideoPath = null!;
    private TextBox txtOutputPath = null!;
    private NumericUpDown numFps = null!;
    private CheckBox chkForceOcr = null!;
    private CheckBox chkTimestamps = null!;
    private CheckBox chkRemoveDuplicates = null!;
    private TextBox txtLanguage = null!;
    private TextBox txtOcrLang = null!;
    private TextBox txtTessData = null!;
    private Button btnProcess = null!;
    private ProgressBar progressBar = null!;
    private TextBox txtLog = null!;
    private Label lblStatus = null!;

    public MainForm()
    {
        InitializeComponent();
        SetupRedirectConsoleOutput();
        DetectAndSetDefaults();
    }

    private void InitializeComponent()
    {
        // Form properties
        this.Text = "Video Text Extraction Tool";
        this.Size = new System.Drawing.Size(800, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new System.Drawing.Size(800, 700);

        // Create controls
        int y = 20;
        int labelWidth = 120;
        int controlX = 140;
        int controlWidth = 620;

        // Video file selection
        var lblVideo = new Label
        {
            Text = "Video File:",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(labelWidth, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblVideo);

        txtVideoPath = new TextBox
        {
            Location = new System.Drawing.Point(controlX, y),
            Size = new System.Drawing.Size(controlWidth - 90, 23),
            ReadOnly = true
        };
        this.Controls.Add(txtVideoPath);

        var btnBrowseVideo = new Button
        {
            Text = "Browse...",
            Location = new System.Drawing.Point(controlX + controlWidth - 85, y),
            Size = new System.Drawing.Size(80, 23)
        };
        btnBrowseVideo.Click += BtnBrowseVideo_Click;
        this.Controls.Add(btnBrowseVideo);

        y += 35;

        // Output file selection
        var lblOutput = new Label
        {
            Text = "Output File:",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(labelWidth, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblOutput);

        txtOutputPath = new TextBox
        {
            Location = new System.Drawing.Point(controlX, y),
            Size = new System.Drawing.Size(controlWidth - 90, 23),
            ReadOnly = true
        };
        this.Controls.Add(txtOutputPath);

        var btnBrowseOutput = new Button
        {
            Text = "Browse...",
            Location = new System.Drawing.Point(controlX + controlWidth - 85, y),
            Size = new System.Drawing.Size(80, 23)
        };
        btnBrowseOutput.Click += BtnBrowseOutput_Click;
        this.Controls.Add(btnBrowseOutput);

        y += 45;

        // Options group box
        var grpOptions = new GroupBox
        {
            Text = "Extraction Options",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 200)
        };
        this.Controls.Add(grpOptions);

        int grpY = 25;

        // Force OCR checkbox
        chkForceOcr = new CheckBox
        {
            Text = "Force OCR (even if subtitles exist)",
            Location = new System.Drawing.Point(15, grpY),
            Size = new System.Drawing.Size(300, 23)
        };
        grpOptions.Controls.Add(chkForceOcr);

        grpY += 30;

        // Include timestamps checkbox
        chkTimestamps = new CheckBox
        {
            Text = "Include timestamps in output",
            Location = new System.Drawing.Point(15, grpY),
            Size = new System.Drawing.Size(300, 23)
        };
        grpOptions.Controls.Add(chkTimestamps);

        grpY += 30;

        // Remove duplicates checkbox
        chkRemoveDuplicates = new CheckBox
        {
            Text = "Remove duplicate text (OCR mode)",
            Location = new System.Drawing.Point(15, grpY),
            Size = new System.Drawing.Size(300, 23),
            Checked = true
        };
        grpOptions.Controls.Add(chkRemoveDuplicates);

        grpY += 35;

        // FPS setting
        var lblFps = new Label
        {
            Text = "Frames/Second (OCR):",
            Location = new System.Drawing.Point(15, grpY + 3),
            Size = new System.Drawing.Size(150, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        grpOptions.Controls.Add(lblFps);

        numFps = new NumericUpDown
        {
            Location = new System.Drawing.Point(170, grpY),
            Size = new System.Drawing.Size(80, 23),
            Minimum = 0.5m,
            Maximum = 10,
            DecimalPlaces = 1,
            Increment = 0.5m,
            Value = 1
        };
        grpOptions.Controls.Add(numFps);

        // Subtitle language
        var lblLang = new Label
        {
            Text = "Subtitle Language:",
            Location = new System.Drawing.Point(270, grpY + 3),
            Size = new System.Drawing.Size(120, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        grpOptions.Controls.Add(lblLang);

        txtLanguage = new TextBox
        {
            Location = new System.Drawing.Point(395, grpY),
            Size = new System.Drawing.Size(80, 23),
            PlaceholderText = "eng"
        };
        grpOptions.Controls.Add(txtLanguage);

        grpY += 35;

        // OCR language
        var lblOcrLang = new Label
        {
            Text = "OCR Language:",
            Location = new System.Drawing.Point(15, grpY + 3),
            Size = new System.Drawing.Size(150, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        grpOptions.Controls.Add(lblOcrLang);

        txtOcrLang = new TextBox
        {
            Location = new System.Drawing.Point(170, grpY),
            Size = new System.Drawing.Size(80, 23),
            Text = "eng"
        };
        grpOptions.Controls.Add(txtOcrLang);

        // Tessdata path
        var lblTessData = new Label
        {
            Text = "Tessdata Path:",
            Location = new System.Drawing.Point(270, grpY + 3),
            Size = new System.Drawing.Size(120, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        grpOptions.Controls.Add(lblTessData);

        txtTessData = new TextBox
        {
            Location = new System.Drawing.Point(395, grpY),
            Size = new System.Drawing.Size(350, 23),
            Text = "./tessdata"
        };
        grpOptions.Controls.Add(txtTessData);

        y += 215;

        // Process button
        btnProcess = new Button
        {
            Text = "Extract Text",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 35),
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnProcess.FlatAppearance.BorderSize = 0;
        btnProcess.Click += BtnProcess_Click;
        this.Controls.Add(btnProcess);

        y += 45;

        // Progress bar
        progressBar = new ProgressBar
        {
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 25),
            Style = ProgressBarStyle.Marquee,
            Visible = false
        };
        this.Controls.Add(progressBar);

        y += 35;

        // Status label
        lblStatus = new Label
        {
            Text = "Ready",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 23),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
        };
        this.Controls.Add(lblStatus);

        y += 30;

        // Log text box
        var lblLog = new Label
        {
            Text = "Processing Log:",
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 20)
        };
        this.Controls.Add(lblLog);

        y += 25;

        txtLog = new TextBox
        {
            Location = new System.Drawing.Point(20, y),
            Size = new System.Drawing.Size(controlWidth + labelWidth, 150),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new System.Drawing.Font("Consolas", 9)
        };
        this.Controls.Add(txtLog);
    }

    private void BtnBrowseVideo_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select Video File",
            Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.wmv;*.flv|All Files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtVideoPath.Text = dialog.FileName;

            // Auto-suggest output path
            if (string.IsNullOrEmpty(txtOutputPath.Text))
            {
                var videoDir = Path.GetDirectoryName(dialog.FileName) ?? "";
                var videoName = Path.GetFileNameWithoutExtension(dialog.FileName);
                txtOutputPath.Text = Path.Combine(videoDir, $"{videoName}_extracted.txt");
            }
        }
    }

    private void BtnBrowseOutput_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Save Output File",
            Filter = "Text Files|*.txt|All Files|*.*",
            DefaultExt = "txt",
            FileName = txtOutputPath.Text
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtOutputPath.Text = dialog.FileName;
        }
    }

    private async void BtnProcess_Click(object? sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(txtVideoPath.Text))
        {
            MessageBox.Show("Please select a video file.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrEmpty(txtOutputPath.Text))
        {
            MessageBox.Show("Please select an output file.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Create processing options
        var options = new ProcessingOptions
        {
            VideoPath = txtVideoPath.Text,
            OutputPath = txtOutputPath.Text,
            FramesPerSecond = (int)numFps.Value,
            ForceOcr = chkForceOcr.Checked,
            IncludeTimestamps = chkTimestamps.Checked,
            RemoveDuplicates = chkRemoveDuplicates.Checked,
            PreferredLanguage = string.IsNullOrWhiteSpace(txtLanguage.Text) ? null : txtLanguage.Text,
            OcrLanguage = txtOcrLang.Text,
            TessDataPath = txtTessData.Text
        };

        // Disable controls during processing
        SetControlsEnabled(false);
        progressBar.Visible = true;
        lblStatus.Text = "Processing...";
        lblStatus.ForeColor = System.Drawing.Color.Blue;
        txtLog.Clear();

        try
        {
            var extractor = new VideoTextExtractor();
            var result = await Task.Run(() => extractor.ExtractTextAsync(options));

            if (result.Success)
            {
                lblStatus.Text = $"Success! Extracted {result.SegmentCount} segments using {result.Method}";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                var message = $"Text extraction completed successfully!\n\n" +
                             $"Method: {result.Method}\n" +
                             $"Segments: {result.SegmentCount}\n" +
                             $"Processing Time: {result.ProcessingTimeSeconds:F2} seconds\n\n" +
                             $"Output saved to:\n{result.OutputPath}\n\n" +
                             $"Would you like to open the output file?";

                var dialogResult = MessageBox.Show(message, "Extraction Complete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = result.OutputPath,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                lblStatus.Text = "Extraction failed!";
                lblStatus.ForeColor = System.Drawing.Color.Red;

                MessageBox.Show($"Extraction failed:\n\n{result.ErrorMessage}",
                    "Extraction Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error occurred!";
            lblStatus.ForeColor = System.Drawing.Color.Red;

            MessageBox.Show($"An error occurred:\n\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBar.Visible = false;
            SetControlsEnabled(true);
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        txtVideoPath.Enabled = enabled;
        txtOutputPath.Enabled = enabled;
        numFps.Enabled = enabled;
        chkForceOcr.Enabled = enabled;
        chkTimestamps.Enabled = enabled;
        chkRemoveDuplicates.Enabled = enabled;
        txtLanguage.Enabled = enabled;
        txtOcrLang.Enabled = enabled;
        txtTessData.Enabled = enabled;
        btnProcess.Enabled = enabled;

        foreach (Control control in this.Controls)
        {
            if (control is Button && control != btnProcess)
            {
                control.Enabled = enabled;
            }
        }
    }

    private void DetectAndSetDefaults()
    {
        // Try to detect Tesseract installation and set default tessdata path
        var possiblePaths = new[]
        {
            @"C:\Program Files\Tesseract-OCR\tessdata",
            @"C:\Program Files (x86)\Tesseract-OCR\tessdata",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tesseract-OCR", "tessdata"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
            Path.Combine(Directory.GetCurrentDirectory(), "tessdata"),
            "./tessdata"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    // Check if eng.traineddata exists in this directory
                    var engFile = Path.Combine(path, "eng.traineddata");
                    if (File.Exists(engFile))
                    {
                        txtTessData.Text = path;
                        txtTessData.ForeColor = System.Drawing.Color.Green;
                        break;
                    }
                }
            }
            catch
            {
                // Ignore errors and continue checking
            }
        }

        // If no valid path found, show warning color
        if (!Directory.Exists(txtTessData.Text) ||
            !File.Exists(Path.Combine(txtTessData.Text, "eng.traineddata")))
        {
            txtTessData.ForeColor = System.Drawing.Color.Red;
        }
    }

    private void SetupRedirectConsoleOutput()
    {
        // Redirect console output to the log textbox
        var writer = new TextBoxWriter(txtLog);
        Console.SetOut(writer);
    }

    private class TextBoxWriter : System.IO.TextWriter
    {
        private readonly TextBox _textBox;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void Write(char value)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.Invoke(new Action(() => Write(value)));
            }
            else
            {
                _textBox.AppendText(value.ToString());
            }
        }

        public override void Write(string? value)
        {
            if (value == null) return;

            if (_textBox.InvokeRequired)
            {
                _textBox.Invoke(new Action(() => Write(value)));
            }
            else
            {
                _textBox.AppendText(value);
            }
        }

        public override void WriteLine(string? value)
        {
            Write(value + Environment.NewLine);
        }
    }
}
