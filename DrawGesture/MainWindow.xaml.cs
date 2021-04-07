using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
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

		private System.Timers.Timer aTimer;
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

		//I don't know what i'm doing, plz help :'(
		public delegate void NextImage();
		public delegate void Label();

		public MainWindow()
		{
			InitializeComponent();
			comboBox1.Items.Add("1.30min(30sx10|1mx5|5mx2|10mx1)");
			comboBox1.Items.Add("2.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("3.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("4.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
			//comboBox1.Items.Add("5.60min(30sx10|1mx5|5mx2|10mx1|30mx1)");
		}

		void OnClickBtnFolder(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog open = new CommonOpenFileDialog
			{
				IsFolderPicker = true
			};

			if (open.ShowDialog() == CommonFileDialogResult.Ok)
			{
				searchFolder = open.FileName;
				var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };
				files = GetFilesFrom(searchFolder, filters, true);

				searchFolder += " | Images: " + files.Length;

				Debug.Write(String.Join("|", files));

				lblFolder.Dispatcher.BeginInvoke(
			   DispatcherPriority.Normal,
			   new Label(ChangeLabelFolder));
			}
		}

		void OnClickBtnStart(object sender, RoutedEventArgs e)
		{
			if ((bool)rTime.IsChecked)
			{
				isClass = false;

				imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

				x = Int32.Parse(tBTime.Text);

				countDown = x;

				SetTimer(1000);

				panel.Visibility = Visibility.Collapsed;
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
						
						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

						x = 30;
						countDown = x;

						times = new int[] { 10, 5, 2, 1, 0};
						timesS = new int[] { oneMin / 2 , oneMin, oneMin * 5, oneMin * 10};

						SetTimer(1000);

						panel.Visibility = Visibility.Collapsed;
						break;
					case "2":
						Debug.Write(selectedState);

						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

						x = 30;
						countDown = x;

						times = new int[] { 10, 5, 2, 1, 1, 0 };
						timesS = new int[] { oneMin / 2, oneMin, oneMin * 5, oneMin * 10, oneHour / 2 };

						SetTimer(1000);

						panel.Visibility = Visibility.Collapsed;
						break;
					default:
						break;
				}
			}
		}

		private void SetTimer(int time)
		{
			aTimer = new System.Timers.Timer(time);
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

			textBox3.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new Label(ChangeCountDown));

			if (!isClass)
			{
				if (countDown == 0)
				{
					countDown = x;

					imageBox.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new NextImage(ChangeImage));
				}
			}
			else 
			{
				if (times[0] == 0)
				{
					aTimer.Enabled = false;
					Debug.Write("DONE!");
					return;
				}

				if (countDown == 0)
				{
					countDown = x;

					imageBox.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new NextImage(ChangeImage));

					nEventsFired++;

					if (nEventsFired == 10 && times[0] == 10)
					{
						UpdateTimer();
					}
					if (nEventsFired == 5 && times[0] == 5)
					{
						UpdateTimer();
					}
					if (nEventsFired == 2 && times[0] == 2)
					{
						UpdateTimer();
					}
					if (nEventsFired == 1 && times[0] == 1)
					{
						UpdateTimer();
					}
				}
			}
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
			textBox1.Text = files[start2];
			count++;
			textBox2.Text = count.ToString();
		}

		private void ChangeCountDown() 
		{
			textBox3.Text = countDown.ToString();
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
