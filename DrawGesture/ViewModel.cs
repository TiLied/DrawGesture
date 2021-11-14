using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

public class ViewModel : ViewModelBase
{
	//https://stackoverflow.com/a/24166629
	//modified
	private readonly bool[] _modeClass = new bool[] { false, false };
	public bool[] ModeClass
	{
		get { return _modeClass; }
	}

	private readonly bool[] _flipV = new bool[] { true, false, false };
	public bool[] FlipV
	{
		get { return _flipV; }
	}
	public int SelectedFlipV
	{
		get { return Array.IndexOf(_flipV, true); }
	}

	private int _imageScaleX = 1;
	public int ImageScaleX
	{
		get => _imageScaleX;
		set => SetProperty(ref _imageScaleX, value);
	}

	public string SelectedPath;
	public string[] Files;
	public List<string> UsedFiles = new List<string>();

	private int _time = 30;
	public int Time{
		get => _time;
		set => SetProperty(ref _time, value);
	}

	private ImageSource _imageS;
	public ImageSource ImageS
	{
		get => _imageS;
		set => SetProperty(ref _imageS, value);
	}

	private string _textFile;
	public string TextFile
	{
		get => _textFile;
		set => SetProperty(ref _textFile, value);
	}

	private string _textCountImage;
	public string TextCountImage
	{
		get => _textCountImage;
		set => SetProperty(ref _textCountImage, value);
	}

	private string _textCount;
	public string TextCount
	{
		get => _textCount;
		set => SetProperty(ref _textCount, value);
	}

	private string _textPause = "Pause";
	public string TextPause
	{
		get => _textPause;
		set => SetProperty(ref _textPause, value);
	}

	private string _textLblFolder = "Please choose folder!";
	public string TextLblFolder
	{
		get => _textLblFolder;
		set => SetProperty(ref _textLblFolder, value);
	}
	
	public List<ClassObj> ClassesEntries { get; }

	private ClassObj _classesEntry;
	public ClassObj ClassesEntry
	{
		get => _classesEntry;
		set => SetProperty(ref _classesEntry, value);
	}
	public ViewModel()
	{
		ClassesEntries = new List<ClassObj>
		{
			new ClassObj
			{
				ClassId=0,
				ClassTextBox="1.30min(30sx10|1mx5|5mx2|10mx1)",
				ClassAmountImg = new int[] {10, 5, 2, 1, 0},
				ClassTime = new int[] { 60000 / 2, 60000, 60000 * 5, 60000 * 10,0 }
				//ClassTimes= new int[,] { { 10, 5, 2, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 0 } }
			},
			new ClassObj
			{
				ClassId=1,
				//5m break before last TODO!
				ClassTextBox="2.60min(30sx10|1mx5|5mx2|10mx1|30mx1)",
				ClassAmountImg = new int[] {10, 5, 2, 1, 1, 0},
				ClassTime = new int[] { 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 60 / 2,0 }
				//ClassTimes=new int[,]{ { 10, 5, 2, 1, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 60 / 2, 0 } }
			},
			new ClassObj
			{
				ClassId=2,
				//14m break before last TODO!
				ClassTextBox="3.2hours(30sx6|1mx3|5mx2|10mx2|20mx1|1.04hx1)",
				ClassAmountImg = new int[] {6, 3, 2, 3, 1, 1, 0},
				ClassTime = new int[] { 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 20, 60000 * 60 + (60000 * 4),0 }
				//ClassTimes=new int[,]{ { 6, 3, 2, 3, 1, 1, 0},{ 60000 / 2, 60000, 60000 * 5, 60000 * 10, 60000 * 20, 60000 * 60 + (60000 * 4), 0 } }
			}
		};
	}
}

public class ClassObj
{
	public int ClassId { get; set; }

	public string ClassTextBox { get; set; }

	public int[] ClassAmountImg { get; set; }
	public int[] ClassTime { get; set; }

	public override string ToString()
	{
		return "ClassId: " + ClassId + "  ClassTextBox: " + ClassTextBox;
	}
}

//https://intellitect.com/getting-started-model-view-viewmodel-mvvm-pattern-using-windows-presentation-framework-wpf/
public abstract class ViewModelBase : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
	{
		if (!EqualityComparer<T>.Default.Equals(field, newValue))
		{
			field = newValue;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
		return false;
	}
}


