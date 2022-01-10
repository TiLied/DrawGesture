using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace DrawGesture
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Timer aTimer;

		private int nEventsFired = 0;
		private int countDown = 0;

		//Still dont know :(
		private delegate void _Delegate();

		private readonly ViewModel viewModel = new();

		private Queue<int> QClassAmountImg { get; set; }
		private Queue<int> QClassTime { get; set; }

		public MainWindow()
		{
			DataContext = viewModel;

			InitializeComponent();
		}

		void OnClickBtnBackToMain(object sender, RoutedEventArgs e)
		{
			//show panels
			endPanel.Visibility = Visibility.Collapsed;
			mainPanel.Visibility = Visibility.Visible;

			//reset count and nEventsFired
			nEventsFired = 0;
			viewModel.UsedFiles.Clear();
			
		}

		void OnClickBtnPause(object sender, RoutedEventArgs e)
		{
			//pause
			if (aTimer.Enabled)
			{
				viewModel.TextPause = "Unpause";
				aTimer.Enabled = false;
			}
			else
			{
				viewModel.TextPause = "Pause";
				aTimer.Enabled = true;
			}
		}

		void OnClickBtnSkip(object sender, RoutedEventArgs e)
		{
			//skip
			if (viewModel.ModeClass[1] == true)
			{
				countDown = QClassTime.Peek() / 1000;

				nEventsFired++;

				int _peek = QClassAmountImg.Peek();

				if (nEventsFired == _peek || _peek == -1)
				{
					UpdateTimer();
					return;
				}

				//change image
				ChangeImage();
			}
			else
			{
				countDown = viewModel.Time;

				//change image
				ChangeImage();
			}
		}

		void OnClickBtnFolder(object sender, RoutedEventArgs e)
		{
			using System.Windows.Forms.FolderBrowserDialog dialog = new();

			System.Windows.Forms.DialogResult result = dialog.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				//formats!
				string[] filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" };

				viewModel.SelectedPath = dialog.SelectedPath;

				//get files
				viewModel.Files = GetFilesFrom(viewModel.SelectedPath, filters, true);

				viewModel.TextLblFolder = viewModel.SelectedPath + " | Images: " + viewModel.Files.Length;
			}
		}

		void OnClickBtnStart(object sender, RoutedEventArgs e)
		{
			if (viewModel.SelectedPath == null) 
			{
				Debug.WriteLine(viewModel.ModeClass[1]);
				Debug.WriteLine("PATH NOT SELECTED!!!");
				return;
			}

			//toggle panels
			mainPanel.Visibility = Visibility.Collapsed;
			imagePanel.Visibility = Visibility.Visible;

			//change image
			ChangeImage();

			if (viewModel.ModeClass[1] == false)
			{
				//set countdown
				countDown = viewModel.Time;
			}

			if (viewModel.ModeClass[1] == true)
			{
				//count images
				nEventsFired++;

				Debug.WriteLine(viewModel.ClassesEntry);

				//add to queue
				QClassAmountImg = new Queue<int>(viewModel.ClassesEntry.ClassAmountImg);
				QClassTime = new Queue<int>(viewModel.ClassesEntry.ClassTime);

				countDown = QClassTime.Peek() / 1000;
			}

			//set timer
			SetTimer(1000);
			return;
		}

		private void FlipV(int v)
		{
			switch (v)
			{
				case 2:
					Random _random = new();
					int _r = _random.Next(0, 2);
					FlipV(_r);
					return;
				case 1:
					viewModel.ImageScaleX = -1;
					return;
				case 0:
				default:
					viewModel.ImageScaleX = 1;
					return;
			}
		}
		private void FlipH(int v)
		{
			switch (v)
			{
				case 2:
					Random _random = new();
					int _r = _random.Next(0, 2);
					FlipH(_r);
					return;
				case 1:
					viewModel.ImageScaleY = -1;
					return;
				case 0:
				default:
					viewModel.ImageScaleY = 1;
					return;
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
			QClassAmountImg.Dequeue();
			QClassTime.Dequeue();

			//reset countDown
			countDown = QClassTime.Peek() / 1000;

			//reset amount img
			nEventsFired = 0;

			if (QClassAmountImg.Peek() > 0)
			{
				if (breakPanel.IsVisible)
				{
					ShowBreakPanel();
				}

				//change image
				ChangeImage();
			}
			else if (QClassAmountImg.Peek() == -1)
			{
				ShowBreakPanel();
			}
			else 
			{
				//stop timer
				aTimer.Enabled = false;
				aTimer.Stop();

				Debug.Write("DONE!");

				//show end panel/screen
				mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ShowEndScreen));

				return;
			}
		}

		private void ShowBreakPanel() 
		{
			if (breakPanel.IsVisible)
			{
				breakPanel.Visibility = Visibility.Collapsed;
			}else
				breakPanel.Visibility = Visibility.Visible;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			//update countDown
			countDown--;

			//update ui countDown
			TimeSpan _time = TimeSpan.FromSeconds(countDown);

			//set textCount to hh\:mm\:ss time
			viewModel.TextCount = _time.ToString(@"hh\:mm\:ss");

			if (viewModel.ModeClass[1] == false)
			{
				if (countDown == 0)
				{
					//reset countDown
					countDown = viewModel.Time;

					//change image
					mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));
				}
				return;
			}

			if (viewModel.ModeClass[1] == true)
			{
				
				if (QClassAmountImg.Peek() == -1)
				{
					//mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ShowBreakPanel));

					//return;
				}

				if (QClassAmountImg.Peek() == 0)
				{
					//stop timer
					aTimer.Enabled = false;
					aTimer.Stop();

					Debug.Write("DONE!");

					//show end panel/screen
					mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ShowEndScreen));

					return;
				}

				if (countDown == 0)
				{
					if (breakPanel.IsVisible)
					{
						mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ShowBreakPanel));
					}

					//reset countDown
					countDown = QClassTime.Peek() / 1000;

					//change image
					mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

					//count images
					nEventsFired++;

					if (nEventsFired == QClassAmountImg.Peek())
					{
						UpdateTimer();
					}
				}

				return;
			}
		}

		private void ShowEndScreen()
		{
			//show endpanel
			imagePanel.Visibility = Visibility.Collapsed;
			endPanel.Visibility = Visibility.Visible;

			imageItems.ItemsSource = viewModel.UsedFiles.ToArray();
			
		}

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Image _s = (Image)sender;

			//https://stackoverflow.com/a/57146975
			//open default image viewer
			Process.Start(new ProcessStartInfo(_s.DataContext.ToString()) { UseShellExecute = true });
		}

		private void ChangeImage()
		{
			FlipV(viewModel.SelectedFlipV);
			FlipH(viewModel.SelectedFlipH);

			Random _random = new();
			
			//get random image(int) from list of files
			int _url = _random.Next(0, viewModel.Files.Length);
			
			BitmapImage bitmap = new();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(viewModel.Files[_url]);
			bitmap.EndInit();
			
			//draw image 
			viewModel.ImageS = bitmap;

			//add image to used!
			viewModel.UsedFiles.Add(bitmap);

			//set ui textFile to file path
			viewModel.TextFile = viewModel.Files[_url];

			//set ui textCountImage
			viewModel.TextCountImage = viewModel.UsedFiles.Count.ToString();
		}

		//
		//https://stackoverflow.com/a/12721673
		private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		//
		//https://stackoverflow.com/a/18321162
		private static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
		{
			List<String> filesFound = new();
			var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			foreach (var filter in filters)
			{
				filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
			}
			return filesFound.ToArray();
		}

	}
}
