using System;
using System.Collections.Generic;
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

using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Threading;
using System.IO;

namespace SpotifyScraper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer timer;

		private String lastParse;

		private Config Config;

		public MainWindow( )
		{
			InitializeComponent( );

			// Load our config or create a new one
			Config = Config.Load( );
			if ( Config == null )
			{
				Config = new Config( );
				Config.Save( );
			}

			this.Loaded += MainWindow_Loaded;

			this.timer = new DispatcherTimer( )
			{
				Interval = TimeSpan.FromMilliseconds( 100 ),
			};

			this.timer.Tick += Timer_Tick;
		}

		private void RefreshProcessList( )
		{
			// Clear out the UI
			comboProcesses.Items.Clear( );

			// Get all the processes and sort them
			List<Process> procressList = new List<Process>( Process.GetProcesses( ) );
			procressList.Sort( ( Process a, Process b ) => { return a.ProcessName.CompareTo( b.ProcessName ); } );

			// List the processes in the UI
			foreach ( Process p in procressList )
				if ( p.MainWindowTitle != "" && p.MainWindowTitle != "Spotify Scraper" )
				{
					comboProcesses.Items.Add(
						new ComboBoxItem( )
						{
							Content = p.ProcessName + ".exe - Preview: '" + p.MainWindowTitle + "'",
							Tag = p.Id
						} );

					// Select the last one that looks like spotify
					if ( p.ProcessName == "Spotify" )
						comboProcesses.SelectedIndex = comboProcesses.Items.Count - 1;
				}
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			RefreshProcessList( );
			textBoxDirectory.Text = Config.NowPlayingPath;
			checkScrollingSpaces.IsChecked = Config.ScrollingSpaces;
			textNumberOfSpaces.Text = Config.NumberOfSpaces.ToString( );
		}

		private void buttonRefresh_Click( object sender, RoutedEventArgs e )
		{
			RefreshProcessList( );
		}

		private void buttonBrowse_Click( object sender, RoutedEventArgs e )
		{
			SaveFileDialog saveFileDialog1 = new SaveFileDialog( )
			{
				Filter = "Text File|*.txt",
				Title = "Choose or Create a Text File",
				FileName = "NowPlaying-Spotify.txt",
				InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
			};

			bool? result = saveFileDialog1.ShowDialog( );
			if ( result.HasValue & result.Value )
			{
				textBoxDirectory.Text = saveFileDialog1.FileName;
				Config.NowPlayingPath = saveFileDialog1.FileName;
				Config.Save( );
			}
		}

		private void buttonStart_Click( object sender, RoutedEventArgs e )
		{
			if ( buttonStart.Content.ToString( ) == "Start" )
			{
				buttonStart.Content = "Stop";
				timer.Start( );

				// Disable UI Elements
				comboProcesses.IsEnabled = false;
				buttonRefresh.IsEnabled = false;
				buttonBrowse.IsEnabled = false;
				textBoxNotPlaying.IsEnabled = false;
				checkScrollingSpaces.IsEnabled = false;
				textNumberOfSpaces.IsEnabled = false;

				lastParse = "";
			}
			else
			{
				buttonStart.Content = "Start";
				timer.Stop( );

				// Enable UI Elements
				comboProcesses.IsEnabled = true;
				buttonRefresh.IsEnabled = true;
				buttonBrowse.IsEnabled = true;
				textBoxNotPlaying.IsEnabled = true;
				checkScrollingSpaces.IsEnabled = true;
				textNumberOfSpaces.IsEnabled = checkScrollingSpaces.IsChecked.Value;
			}
		}

		private void Timer_Tick( object sender, EventArgs e )
		{
			ComboBoxItem selectedItem = comboProcesses.SelectedItem as ComboBoxItem;
			if ( selectedItem == null )
			{
				// TODO: Report no process selected.
				return;
			}

			Process process = null;
			try
			{
				 process = Process.GetProcessById( (int) selectedItem.Tag );
			}
			catch { }

			if ( process == null )
			{
				// TODO: Report no process found, select a different process.
				return;
			}

			string title = process.MainWindowTitle;
			if ( title == null )
			{
				// TODO: Report null title.
				return;
			}

			if ( lastParse == title )
			{
				// TODO: Verbose, report unchanged title. 
				return;
			}
			lastParse = title;

			// If spotify is not playing anything, replace it with user text
			if ( title == "Spotify" )
				title = textBoxNotPlaying.Text;

			title += new String( ' ', Config.ScrollingSpaces ? (int)Config.NumberOfSpaces : 0 );

			// Write the title to the file;
			File.WriteAllText( Config.NowPlayingPath, title );
		}

		private void textNumberOfSpaces_TextChanged( object sender, TextChangedEventArgs e )
		{
			try
			{
				Config.NumberOfSpaces = Convert.ToUInt32( textNumberOfSpaces.Text );
				Config.Save( );
			}
			catch { }
		}

		private void textNumberOfSpaces_LostFocus( object sender, RoutedEventArgs e )
		{
			textNumberOfSpaces.Text = Config.NumberOfSpaces.ToString( );
		}

		private void checkScrollingSpaces_CheckChanged( object sender, RoutedEventArgs e )
		{
			textNumberOfSpaces.IsEnabled = checkScrollingSpaces.IsChecked.Value;
			Config.ScrollingSpaces = checkScrollingSpaces.IsChecked.Value;
			Config.Save( );
		}
	}
}
