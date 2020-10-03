using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.Threading;
using System.Text.RegularExpressions;

namespace DrawGesture
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Timers.Timer aTimer;
		private System.Timers.Timer aTimer2;
		private int[] times;
		private int[] timesS;
		private string[] files;
		private String searchFolder;
		private static int nEventsFired = 0;
		private int oneMin = 60000;
		//private int tenMin = 60000 * 10;
		//private int halfHour = 60000 * 30;
		private int oneHour = 60000 * 60;
		private int count = 0;

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
				files = GetFilesFrom(searchFolder, filters, false);

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
				imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

				int x = Int32.Parse(tBTime.Text);
				SetTimer(x * 1000);

				panel.Visibility = Visibility.Collapsed;
			}

			if ((bool)rClass.IsChecked)
			{
				string selectedState = comboBox1.SelectedItem.ToString();
				selectedState = selectedState.Substring(0, 1);
				switch (selectedState) 
				{
					case "1":
						Debug.Write(selectedState);
						
						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

						times = new int[] { 10, 5, 2, 1, 0};
						timesS = new int[] { oneMin / 2 , oneMin, oneMin * 5, oneMin * 10};
						aTimer2 = new System.Timers.Timer(timesS[0]);
						aTimer2.Elapsed += OnTimedEvent2;
						aTimer2.AutoReset = true;
						aTimer2.Enabled = true;
						
						panel.Visibility = Visibility.Collapsed;
						break;
					case "2":
						Debug.Write(selectedState);

						imageBox.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new NextImage(ChangeImage));

						times = new int[] { 10, 5, 2, 1, 1, 0};
						timesS = new int[] { oneMin / 2, oneMin, oneMin * 5, oneMin * 10, oneHour/2};
						aTimer2 = new System.Timers.Timer(timesS[0]);
						aTimer2.Elapsed += OnTimedEvent2;
						aTimer2.AutoReset = true;
						aTimer2.Enabled = true;

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
			aTimer2.Enabled = false;
			times = times.Skip(1).ToArray();
			timesS = timesS.Skip(1).ToArray();
			aTimer2 = new System.Timers.Timer(timesS[0]);
			aTimer2.Elapsed += OnTimedEvent2;
			aTimer2.AutoReset = true;
			aTimer2.Enabled = true;
			nEventsFired = 0;
		}

		private void OnTimedEvent2(Object source, ElapsedEventArgs e)
		{
			if (times[0] == 0)
			{
				aTimer2.Enabled = false;
				Debug.Write("DONE!");
				return;
			}

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

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			imageBox.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new NextImage(ChangeImage));
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

		//https://stackoverflow.com/a/12721673
		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

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
