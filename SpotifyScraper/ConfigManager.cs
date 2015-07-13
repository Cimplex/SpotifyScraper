using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace SpotifyScraper
{
	[DataContract]
	public class Config
	{
		[DataMember]
		public String NowPlayingPath
		{
			get;
			set;
		}

		[DataMember]
		public Boolean ScrollingSpaces
		{
			get;
			set;
		}

		[DataMember]
		public UInt32 NumberOfSpaces
		{
			get;
			set;
		}

		public static String ConfigDirectory
		{
			get { return Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) + "\\SpotifyScraper"; }
		}

		public static String ConfigPath
		{
			get
			{
				return ConfigDirectory + "\\config.xml";
			}
		}

		public Config( )
		{
			// Default Values
			NowPlayingPath = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\NowPlaying-Spotify.txt";
			ScrollingSpaces = false;
			NumberOfSpaces = 10;
		}

		public void Save( )
		{
			XmlSerializer serializer = new XmlSerializer( typeof( Config ) );
			XmlDocument doc = new XmlDocument( );

			XPathNavigator nav = doc.CreateNavigator( );
			using ( XmlWriter writer = nav.AppendChild( ) )
			{
				serializer.Serialize( writer, this );
			}

			// Debug XML Things
			XmlWriterSettings Settings = new XmlWriterSettings( )
			{
				Indent = true,
				IndentChars = "    ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
			};

			if ( !Directory.Exists( ConfigDirectory ) )
				Directory.CreateDirectory( ConfigDirectory );

			using ( FileStream file = new FileStream( ConfigPath, FileMode.Create ) )
			{
				using ( XmlWriter writer = XmlWriter.Create( file, Settings ) )
				{
					doc.WriteTo( writer );
					writer.Flush( );
				}
			}
		}

		public static Config Load( )
		{
			Config config = null;

			try
			{
				using ( FileStream file = new FileStream( ConfigPath, FileMode.Open ) )
				{
					using ( XmlReader reader = XmlReader.Create( file ) )
					{
						XmlSerializer serializer = new XmlSerializer( typeof( Config ) );
						config = (Config) serializer.Deserialize( reader );
					}
				}

				return config;
			}
			catch
			{
				return null;
			}
		}
	}

}
