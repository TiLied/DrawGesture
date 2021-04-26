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
		private class ClassObj
		{
			public int ClassId { get; set; }
			public int[,] ClassTimes { get; set; }

			public string ClassTextBox { get; set; }

			public override string ToString()
			{
				return "ClassId: " + ClassId + "   ClassTimes: " + ClassTimes + "   ClassTextBox: " + ClassTextBox;
			}
		}

		// Create a list of classes.
		private readonly List<ClassObj> _classes = new List<ClassObj>
		{
			new ClassObj
			{
				ClassId=0,
				ClassTextBox="1.30min(30sx10|1mx5|5mx2|10mx1)",
				ClassTimes= new int[,] { { 10, 5, 2, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 0 } }
			},
			new ClassObj
			{
				ClassId=1,
				//5m break before last TODO
				ClassTextBox="2.60min(30sx10|1mx5|5mx2|10mx1|30mx1)",
				ClassTimes=new int[,]{ { 10, 5, 2, 1, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 60 / 2, 0 } }
			},
			new ClassObj
			{
				ClassId=2,
				//14m break before last TODO
				ClassTextBox="3.2hours(30sx6|1mx3|5mx2|10mx2|20mx1|1.04hx1)",
				ClassTimes=new int[,]{ { 6, 3, 2, 3, 1, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 20, 60000 * 60 + (60000 * 4), 0 } }
			}
		};

		private Timer aTimer;
		private int[,] times;
		private string[] files;

		private string searchFolder = "";
		private int nEventsFired = 0;
		private int count = 0;
		private int countDown = 0;
		private int intervalNumber = 0;
		private bool isClass = false;

		//Still dont know :(
		private delegate void _Delegate();

		public MainWindow()
		{
			InitializeComponent();
			
			//Disabal panels
			imagePanel.Visibility = Visibility.Collapsed;

			foreach (ClassObj aClass in _classes)
			{
				Debug.WriteLine(aClass.ToString());

				//add Textboxes to ui
				comboBox1.Items.Add(aClass.ClassTextBox);

				foreach (int i in aClass.ClassTimes)
				{
					Debug.WriteLine("{0} ", i);
				}
			}
		}

		void OnClickBtnPause(object sender, RoutedEventArgs e)
		{
			//pause
			if ((bool)aTimer.Enabled)
			{
				aTimer.Enabled = false;
			}
			else
			{
				aTimer.Enabled = true;
			}
		}

		void OnClickBtnSkip(object sender, RoutedEventArgs e)
		{
			//skip
			countDown = intervalNumber;

			//change image
			mainWindow.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new _Delegate(ChangeImage));

			if (isClass)
			{
				nEventsFired++;

				if (nEventsFired == times[0,0])
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
				
				//get files
				files = GetFilesFrom(searchFolder, filters, true);

				searchFolder += " | Images: " + files.Length;

				mainWindow.Dispatcher.BeginInvoke(
			   DispatcherPriority.Normal,
			   new _Delegate(ChangeLabelFolder));
			}
		}

		void OnClickBtnStart(object sender, RoutedEventArgs e)
		{
			if ((bool)rClass.IsChecked)
			{
				//set is Class mode
				isClass = true;

				//toggle panels
				mainPanel.Visibility = Visibility.Collapsed;
				imagePanel.Visibility = Visibility.Visible;

				string _selectedState = comboBox1.SelectedItem.ToString();

				foreach (ClassObj aClass in _classes)
				{
					if (_selectedState == aClass.ClassTextBox) 
					{
						Debug.WriteLine(_selectedState);

						//change image
						mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

						//clone 
						times = (int[,])aClass.ClassTimes.Clone();

						//set countdown
						countDown = times[1,0] / 1000;

						//set intervalNumber
						intervalNumber = countDown;

						//set timer
						SetTimer(1000);
					}
				}
			}
			else 
			{
				//set is Class mode
				isClass = false;

				//toggle panels
				mainPanel.Visibility = Visibility.Collapsed;
				imagePanel.Visibility = Visibility.Visible;

				//change image
				mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new _Delegate(ChangeImage));

				//set countdown
				countDown = Int32.Parse(tBTime.Text);

				//set intervalNumber
				intervalNumber = countDown;

				//set timer
				SetTimer(1000);
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
			int[] _indices = new int[] { 0 };

			times = ResizeArray<int>(times, _indices);

			countDown = times[1,0] / 1000;
			intervalNumber = times[1, 0] / 1000;

			nEventsFired = 0;
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			//update countDown
			countDown--;

			//update ui countDown
			mainWindow.Dispatcher.BeginInvoke(
				DispatcherPriority.Normal,
				new _Delegate(ChangeCountDown));

			if (!isClass)
			{
				//check if countDown is 0, yes change image
				if (countDown == 0)
				{
					//reset countDown
					countDown = intervalNumber;

					//change image
					mainWindow.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new _Delegate(ChangeImage));
				}

			}
			else 
			{
				if (times[0,0] == 0)
				{
					//stop timer
					aTimer.Enabled = false;
					aTimer.Stop();

					Debug.Write("DONE!");

					//start all over again
					mainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,new _Delegate(StartOver));

					return;
				}

				//check if countDown is 0, yes change image
				if (countDown == 0)
				{
					//reset countDown
					countDown = intervalNumber;
					
					//change image
					mainWindow.Dispatcher.BeginInvoke(
						DispatcherPriority.Normal,
						new _Delegate(ChangeImage));

					nEventsFired++;

					if (nEventsFired == times[0,0])
					{
						UpdateTimer();
					}
				}
			}
		}

		private void StartOver()
		{
			//show panels
			imagePanel.Visibility = Visibility.Collapsed;
			mainPanel.Visibility = Visibility.Visible;

			//reset count and nEventsFired
			nEventsFired = 0;
			count = 0;
		}

		private void ChangeLabelFolder()
		{
			lblFolder.Content = searchFolder;
		}

		private void ChangeImage()
		{
			Random _random = new Random();
			
			//get random image(int) from list of files
			int _url = _random.Next(0, files.Length);
			
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(files[_url]);
			bitmap.EndInit();
			
			//draw image 
			imageBox.Source = bitmap;
			
			//set ui textFile to file path
			textFile.Text = files[_url];

			//count images
			count++;

			//set ui textCountImage
			textCountImage.Text = count.ToString();
		}

		private void ChangeCountDown()
		{
			TimeSpan _time = TimeSpan.FromSeconds(countDown);

			//set textCount to hh\:mm\:ss time
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

		//Delete col in multiArray
		//https://social.msdn.microsoft.com/Forums/vstudio/en-US/137cbf2d-fbc3-45e0-a6ba-cd9bdc90b304/deleting-columns-from-multidimensional-array
		private T[,] ResizeArray<T>(T[,] original, int[] columnsToRemove)
		{
			var newArray = new T[original.GetLength(0), original.GetLength(1) - columnsToRemove.Length];
			int minRows = original.GetLength(0);
			//int minCols = Math.Min(columnsToRemove.Length, original.GetLength(1));
			for (int i = 0; i < minRows; i++)
			{
				int currentColumn = 0;
				for (int j = 0; j < original.GetLength(1); j++)
				{
					if (columnsToRemove.Contains(j) == false)
					{
						newArray[i, currentColumn] = original[i, j];
						currentColumn++;
					}
				}
			}
			return newArray;
		}
	}
}
