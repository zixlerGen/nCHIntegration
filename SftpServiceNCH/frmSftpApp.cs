using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;

namespace SftpServiceNCH
{
    public partial class frmSftpApp : Form
    {
        private readonly SftpSettings _sftpSettings;
        private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private readonly string _logFilePath;
        private System.Windows.Forms.Timer _cycleTimer;
        private bool _isCycleRunning = false;
        public frmSftpApp()
        {
            InitializeComponent();

            // Start the timer (e.g., every 5 minutes)
            _cycleTimer = new System.Windows.Forms.Timer();
            _cycleTimer.Interval = 5000; // 5 minutes in milliseconds
            _cycleTimer.Tick += async (sender, e) => await RunCycleAsync();
            _cycleTimer.Start();

            // Initialize log directory and file 
            _logFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyyMMdd}.log");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            DeleteOldLogFiles();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Fix: Add null check to ensure the section exists before calling Get<T>()
            var sftpSettingsSection = config.GetSection("SftpSettings");
            if (sftpSettingsSection.Exists())
            {
                _sftpSettings = sftpSettingsSection.Get<SftpSettings>() ?? throw new InvalidOperationException("SftpSettings section is missing or invalid in the configuration.");
            }
            else
            {
                throw new InvalidOperationException("SftpSettings section is missing in the configuration.");
            }

            Log("Application started.");
        }
        private async Task RunCycleAsync()
        {
            if (_isCycleRunning) return; // Prevent overlapping cycles
            _isCycleRunning = true;
            try
            {
                // You can call your upload logic here
                Log($"Checking Files...");
                await this.InvokeAsync(async () => SftpUpload());
            }
            finally
            {
                _isCycleRunning = false;
            }
        }
        private void Log(string message)
        {
            string logEntry = $"{DateTime.Now:HH:mm:ss}: {message}\r\n";
            // Log to UI
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), message);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss}: {message}\r\n");
                txtLog.ScrollToCaret();
            }

            // Log to file (separate file per day)
            try
            {
                string dailyLogFilePath = Path.Combine(_logDirectory, $"{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(dailyLogFilePath, logEntry);
            }
            catch (Exception ex)
            {
                // Optionally handle file logging errors (e.g., show in UI)
                txtLog.AppendText($"Log file error: {ex.Message}\r\n");
            }
        }
        private void DeleteOldLogFiles()
        {
            try
            {
                var files = Directory.GetFiles(_logDirectory, "*.log");
                foreach (var file in files)
                {
                    var creationTime = File.GetCreationTime(file);
                    if ((DateTime.Now - creationTime).TotalDays > 90)
                    {
                        File.Delete(file);
                        Log($"Deleted old log file: {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally handle errors (e.g., log to UI)
                txtLog.AppendText($"Log cleanup error: {ex.Message}\r\n");
            }
        }
        private void UpdateProgress(double percentage)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(percentage)));
                return;
            }
            //progressBar.Value = (int)percentage;
            progressBar.Value = Math.Min(progressBar.Maximum, Math.Max(progressBar.Minimum, (int)percentage));
            lblProgress.Text = $"{percentage:F0}%";
            ;
        }
        private void SetStatus(string status, System.Drawing.Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(status, color)));
                return;
            }
            lblStatus.Text = status;
            lblStatus.ForeColor = color;
        }
        private void EnableControls(bool enable)
        {
            // Ensure UI update is on the UI thread
            if (InvokeRequired)
            {
                Invoke(new Action(() => EnableControls(enable)));
                return;
            }
            //btnBrowse.Enabled = enable;
            //txtLocalFilePath.Enabled = enable;
            //txtSftpHost.Enabled = enable;
            //txtSftpPort.Enabled = enable;
            //txtUsername.Enabled = enable;
            //txtRemotePath.Enabled = enable;
            btnUpload.Enabled = enable;

            // Enable/disable authentication specific controls based on current selection
            //rdoAuthPassword.Enabled = enable;
            //rdoAuthKey.Enabled = enable;
            //UpdateAuthControlsVisibility(enable); // Re-evaluate visibility/enabled state
        }
        private void UpdateAuthControlsVisibility(bool mainControlsEnabled = true)
        {
            // Password controls
            //lblPassword.Visible = rdoAuthPassword.Checked;
            //txtPassword.Visible = rdoAuthPassword.Checked;
            //txtPassword.Enabled = rdoAuthPassword.Checked && mainControlsEnabled;

            //// Key file controls
            //lblKeyFile.Visible = rdoAuthKey.Checked;
            //txtKeyFilePath.Visible = rdoAuthKey.Checked;
            //btnBrowseKey.Visible = rdoAuthKey.Checked;
            //lblKeyPassphrase.Visible = rdoAuthKey.Checked;
            //txtKeyPassphrase.Visible = rdoAuthKey.Checked;

            //txtKeyFilePath.Enabled = rdoAuthKey.Checked && mainControlsEnabled;
            //btnBrowseKey.Enabled = rdoAuthKey.Checked && mainControlsEnabled;
            //txtKeyPassphrase.Enabled = rdoAuthKey.Checked && mainControlsEnabled;
        }
        //private void BtnBrowse_Click(object sender, EventArgs e)
        //{
        //    using (OpenFileDialog openFileDialog = new OpenFileDialog())
        //    {
        //        if (openFileDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            txtLocalFilePath.Text = openFileDialog.FileName;
        //        }
        //    }
        //}
        //private void BtnBrowseKey_Click(object sender, EventArgs e)
        //{
        //    using (OpenFileDialog openFileDialog = new OpenFileDialog())
        //    {
        //        openFileDialog.Filter = "SSH Private Key Files (*.ppk;*.pem;*key)|*.ppk;*.pem;*.key|All files (*.*)|*.*";
        //        if (openFileDialog.ShowDialog() == DialogResult.OK)
        //        {
        //            txtKeyFilePath.Text = openFileDialog.FileName;
        //        }
        //    }
        //}
        private void RdoAuthMethod_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthControlsVisibility();
        }
        private void BtnUpload_Click(object sender, EventArgs e)
        {
            SftpUpload();
        }

        private async void SftpUpload()
        {
            // check if there are files in Main to upload
            var filePaths = GetAllFilesInDirectory(_sftpSettings.sftpLocalPath + "\\Main"); // Get all file names from the local path
            
            if(filePaths.Count == 0)
                filePaths = GetAllFilesInDirectory(_sftpSettings.sftpLocalPath + "\\Details");

            if (filePaths.Count == 0)
            {
                Log($"No file upload...");
                return; // No files to upload
            }
            // Basic validation
            //if (string.IsNullOrWhiteSpace(txtLocalFilePath.Text) || !File.Exists(txtLocalFilePath.Text))
            //{
            //    SetStatus("Please select a valid local file.", System.Drawing.Color.Red);
            //    Log("Error: No local file selected or file does not exist.");
            //    return;
            //}
            if (string.IsNullOrWhiteSpace(_sftpSettings.sftpHost) || string.IsNullOrWhiteSpace(_sftpSettings.sftpusername) || string.IsNullOrWhiteSpace(_sftpSettings.sftpRemotePath))
            {
                SetStatus("Please fill in all SFTP connection details.", System.Drawing.Color.Red);
                Log("Error: Missing SFTP connection details.");
                return;
            }
            if (_sftpSettings.sftpport <= 0)
            {
                SetStatus("Please enter a valid SFTP port number.", System.Drawing.Color.Red);
                Log("Error: Invalid SFTP port.");
                return;
            }

            EnableControls(false); // Disable controls during upload
            SetStatus("Uploading file...", System.Drawing.Color.Blue);
            Log($"Starting upload for: {_sftpSettings.sftpLocalPath}");
            UpdateProgress(0);

            try
            {
                await Task.Run(() => // Run SFTP operation on a background thread
                {
                    SftpClient client;

                    string keyFilePath = _sftpSettings.sftpPrivateKeyPath;
                    string keyPassphrase = ""; // Can be empty if no passphrase

                    // Create PrivateKeyFile object. Passphrase can be null if not used.
                    PrivateKeyFile privateKeyFile;
                    if (string.IsNullOrEmpty(keyPassphrase))
                    {
                        privateKeyFile = new PrivateKeyFile(keyFilePath);
                    }
                    else
                    {
                        privateKeyFile = new PrivateKeyFile(keyFilePath, keyPassphrase);
                    }

                    client = new SftpClient(_sftpSettings.sftpHost, _sftpSettings.sftpport, _sftpSettings.sftpusername, privateKeyFile);
                    Log($"Using SSH Key authentication with key: {Path.GetFileName(keyFilePath)}.");

                    using (client) // Ensure client is disposed
                    {
                        Log("Connecting to SFTP server...");
                        client.Connect();
                        Log("Connected to SFTP server.");

                        // Ensure remote directory exists
                        string remoteDirectory = _sftpSettings.sftpRemotePath;
                        Log("Remote Directory: " + remoteDirectory);
                        if (string.IsNullOrEmpty(remoteDirectory)) remoteDirectory = "/";

                        if (!client.Exists(remoteDirectory))
                        {
                            Log($"Remote directory '{remoteDirectory}' does not exist. Creating...");
                            client.CreateDirectory(remoteDirectory);
                            Log($"Remote directory '{remoteDirectory}' created.");
                        }

                        int fileCount = filePaths.Count;
                        for (int i = 0; i < fileCount; i++)
                        {
                            string localFilePath = filePaths[i].Trim();
                            if (!File.Exists(localFilePath))
                            {
                                Log($"File not found: {localFilePath}");
                                continue;
                            }
                            using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                            {
                                string remoteFileName = Path.GetFileName(localFilePath);
                                string finalRemotePath = Path.Combine(remoteDirectory, remoteFileName).Replace("\\", "/");
                                Log($"Uploading '{remoteFileName}' to '{finalRemotePath}'...");

                                long fileLength = fileStream.Length;
                                client.UploadFile(fileStream, finalRemotePath, (uploadedBytes) =>
                                {
                                    double percentage = ((double)i / fileCount * 100) + ((double)uploadedBytes / fileLength * (100.0 / fileCount));
                                    UpdateProgress(percentage);
                                });
                            }
                            try
                            {
                                File.Delete(localFilePath);
                                Log($"Local file '{localFilePath}' deleted after successful upload.");
                            }
                            catch (Exception ex)
                            {
                                Log($"Failed to delete local file '{localFilePath}': {ex.Message}");
                                Log($"Stack Trace: {ex.StackTrace}");
                            }
                        }
                        client.Disconnect();
                        Log("Disconnected from SFTP server.");
                    }
                });

                SetStatus("All files uploaded successfully!", System.Drawing.Color.Green);
                Log("All file uploads completed successfully.");
            }
            catch (SshAuthenticationException authEx)
            {
                SetStatus($"Authentication failed: {authEx.Message}", System.Drawing.Color.Red);
                Log($"Authentication Error: {authEx.Message}");
                Log($"Stack Trace: {authEx.StackTrace}");
            }
            catch (Exception ex)
            {
                SetStatus($"Upload failed: {ex.Message}", System.Drawing.Color.Red);
                Log($"Upload Error: {ex.Message}");
                Log($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                EnableControls(true); // Re-enable controls
            }
        }
        private void frmSftpApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ensure any necessary cleanup or final logging
            Log("Application is closing.");
            DeleteOldLogFiles(); // Optional: Clean up logs on exit
        }

        private void btnStartCycle_Click(object sender, EventArgs e)
        {
            if (_cycleTimer == null)
                _cycleTimer = new System.Windows.Forms.Timer();
                _cycleTimer.Interval = 5000; // 5 minutes in milliseconds
                _cycleTimer.Tick += async (sender, e) => await RunCycleAsync();
                _cycleTimer.Start();
        }

        private void btnStopCycle_Click(object sender, EventArgs e)
        {
            _cycleTimer?.Stop();
            _cycleTimer = null;
        }
        // Returns all file names (not full paths) from a semicolon-separated file path string
        private List<string> GetAllFilesInDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                return new List<string>();

            // Get all files (full paths) in the directory (non-recursive)
            return Directory.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly).ToList();
        }
    }
}
