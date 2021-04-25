using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace DrawGesture
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly int oneMin = 60000;
		//private readonly int tenMin = 60000 * 10;
		//private readonly int halfHour = 60000 * 30;
		private readonly int oneHour = 60000 * 60;

		private Timer aTimer;
		private int[] times;
		private int[] timesS;
		private string[] files;
		private String searchFolder;
		private static int nEventsFired = 0;
		private int count = 0;

		private int countDown = 0;
		private int x;
		private bool isClass = false;

		//private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

		//Still dont know :(
		private delegate void _Delegate();

		public MainWindow()
		{
			InitializeComponent();

			imagePanel.Visibility = Visibility.Collapsed;
			
			comboBox1.Items.Add("1.30min(30sx10|1mx5|5mx2|10mx1)");
			comboBox1.Items.Add("2.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("3.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("4.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("5.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
		}

		void OnClickBtnSkip(object sender, RoutedEventArgs e)
		{
			countDown = x;

			imageBox.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new _Delegate(ChangeImage));

			if (isClass)
			{
				nEventsFired++;

				if (nEventsFired == times[0])
				{
					UpdateTimer();
				}
			}
		}

		void OnClickBtnFolder(object sender, RoutedEventArgs e)
		{
			using var dialog = new System.Windows.Forms.FolderBrowserDialog();

			System.Windows.Forms.DialogResult result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				searchFolder = dialog.SelectedPath;
				var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
				files = GetFilesFrom(searchFolder, filters, true);

				searchFolder += " | Images: " + files.Length;

				Debug.Write(String.Join("|", files));

				lblFolder.Dispatcher.BeginInvoke(
			   DispatcherPriority.Normal,
			   new _Delegate(ChangeLabelFolder));
			}
		}

		void OnClickBtnStart(object sender, RoutedEventArgs e)
		{
			if ((bool)rTime.IsChecked)
			{
				isClass = false;

				imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

				x = Int32.Parse(tBTime.Text);

				countDown = x;

				SetTimer(1000);

				mainPanel.Visibility = Visibility.Collapsed;
				imagePanel.Visibility = Visibility.Visible;
			}

			if ((bool)rClass.IsChecked)
			{
				isClass = true;

				string selectedState = comboBox1.SelectedItem.ToString();
				selectedState = selectedState.Substring(0, 1);

				switch (selectedState) 
				{
					case "1":
						Debug.Write(selectedState);
						
						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

						x = 30;
						countDown = x;

						times = new int[] { 10, 5, 2, 1, 0};
						timesS = new int[] { oneMin / 2 , oneMin, oneMin * 5, oneMin * 10, 0};

						SetTimer(1000);

						mainPanel.Visibility = Visibility.Collapsed;
						imagePanel.Visibility = Visibility.Visible;
						break;
					case "2":
						Debug.Write(selectedState);

						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

						x = 30;
						countDown = x;

						times = new int[] { 10, 5, 2, 1, 1, 0 };
						timesS = new int[] { oneMin / 2, oneMin, oneMin * 5, oneMin * 10, oneHour / 2, 0};

						SetTimer(1000);

						mainPanel.Visibility = Visibility.Collapsed;
						imagePanel.Visibility = Visibility.Visible;
						break;
					default:
						break;
				}
			}
		}

		private void SetTimer(int time)
		{
			aTimer = new Timer(time);
			aTimer.Elapsed += OnTimedEvent;
			aTimer.AutoReset = true;
			aTimer.Enabled = true;
		}

		private void UpdateTimer()
		{
			times = times.Skip(1).ToArray();
			timesS = timesS.Skip(1).ToArray();

			countDown = timesS[0] / 1000;
			x = timesS[0] / 1000;

			nEventsFired = 0;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			countDown--;

			textCount.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new _Delegate(ChangeCountDown));

			if (!isClass)
			{
				if (countDown == 0)
				{
					countDown = x;

					imageBox.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new _Delegate(ChangeImage));
				}
			}
			else 
			{
				if (times[0] == 0)
				{
					aTimer.Enabled = false;

					Debug.Write("DONE!");

					imagePanel.Dispatcher.BeginInvoke(
	DispatcherPriority.Normal,
	new _Delegate(StartOver));

					return;
				}

				if (countDown == 0)
				{
					countDown = x;

					imageBox.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new _Delegate(ChangeImage));

					nEventsFired++;

					if (nEventsFired == times[0])
					{
						UpdateTimer();
					}
				}
			}
		}

		private void StartOver() 
		{
			imagePanel.Visibility = Visibility.Collapsed;
			mainPanel.Visibility = Visibility.Visible;
			nEventsFired = 0;
			count = 0;
		}

		private void ChangeLabelFolder()
		{
			lblFolder.Content = searchFolder;
		}

		private void ChangeImage()
		{
			Random random = new Random();
			int start2 = random.Next(0, files.Length);
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(files[start2]);
			bitmap.EndInit();
			imageBox.Source = bitmap;
			textFile.Text = files[start2];
			count++;
			textCountImage.Text = count.ToString();
		}

		private void ChangeCountDown() 
		{
			TimeSpan _time = TimeSpan.FromSeconds(countDown);
			textCount.Text = _time.ToString(@"hh\:mm\:ss");
		}

		//
		//https://stackoverflow.com/a/12721673
		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		//
		//https://stackoverflow.com/a/18321162
		private static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
		{
			List<String> filesFound = new List<String>();
			var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			foreach (var filter in filters)
			{
				filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
			}
			return filesFound.ToArray();
		}

	}
}
