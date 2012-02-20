using System;
using System.Drawing;
using System.Globalization;
using System.Json;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MoviesInfo
{
	public partial class Form1 : Form
	{
		// Change URL service according to your configuration
		private const string MashupServiceEndpoint = "http://localhost:2244/Movies/Info";
		
	
		private static string PrepareServiceUrl(string movie, int year, string lang)
		{
			var sb = new StringBuilder(MashupServiceEndpoint);
			sb.Append("?t=");
			sb.Append(movie);
			if (year != 0)
			{
				sb.Append("&y=");
				sb.Append(year);
			}
			if (!String.IsNullOrEmpty(lang))
			{
				sb.Append("&l=");
				sb.Append(lang);
			}
			return sb.ToString();
		}

		public Form1()
		{
			InitializeComponent();
			posterBox.SizeMode = PictureBoxSizeMode.StretchImage;
		}

		private void ClearInput()
		{
			plotBox.Text = "";
			titleBox.Text = "";
			yearBox.Text = "";
			directorBox.Text = "";
			 
			if (posterBox.Image != null)
			{
				posterBox.Image.Dispose();
				posterBox.Image = null;

			}
			foreach (PictureBox b in photosPanel.Controls)
			{
				b.Dispose();
			}
			photosPanel.Controls.Clear();
			reviewsList.Items.Clear();
		}

		//Start a new search
		private void searchButton_Click(object sender, EventArgs e)
		{
			int year=0;

			if (String.IsNullOrEmpty(movieIn.Text))
			{
				MessageBox.Show("Must enter movie!");
				return;
			}
			if (!String.IsNullOrEmpty(yearIn.Text) &&
				!int.TryParse(yearIn.Text, NumberStyles.Integer, null,out year))
			{
				MessageBox.Show("Invalid year!");
				return;
			}
			ClearInput();
			searchButton.Enabled = false;
			GetMovieInfo(movieIn.Text, year, langIn.Text);
		}

		private JsonValue GetMovie(string title, int year, string lang)
		{
			var  httpClient = new HttpClient();
			var url = PrepareServiceUrl(title, year,lang);
			var content = httpClient.GetAsync(url).Result.Content;
			return content.ReadAsAsync<JsonValue>().Result;
		}

		private Image LoadImageFromUrl(string location)
		{
			var httpClient = new HttpClient(); 
			var stream = httpClient.GetStreamAsync(location).Result;
			return Image.FromStream(stream);
		}

		// Change json names according to your result
		private void GetMovieInfo(string title, int year, string lang)
		{
			SynchronizationContext guiCtx = SynchronizationContext.Current;
			
			Task.Factory.StartNew(() =>
			{
				JsonValue jmovie = GetMovie(title, year,lang);
				dynamic json = jmovie.AsDynamic();

				// Show generic info
				guiCtx.Post(_ =>
				{
					plotBox.Text = json.movie.synopsis;
					titleBox.Text = json.movie.title;
					directorBox.Text = json.movie.director;
					yearBox.Text = json.movie.year;

					// Get Poster
					var image = LoadImageFromUrl((string)json.movie.poster);
					posterBox.Image = image;

					// Show Reviews 
					foreach (dynamic review in json.movie.critics)
					{
						var item = reviewsList.Items.Add((string)review.author);
						item.SubItems.Add((string)review.capsule_review);
						item.SubItems.Add((string)review.reference);
					}
				},null);
				

				// Show Related Photos
			 
				JsonArray photos = json.movie.photos;
				foreach (dynamic photo in photos)
				{
						
					Image img = LoadImageFromUrl((string)photo);
					guiCtx.Post(_ =>
					{
						// Create picture box
						var pbx = new PictureBox();
						photosPanel.Controls.Add(pbx);
						pbx.BorderStyle = BorderStyle.Fixed3D;
						pbx.Width = 200;
						pbx.Dock = DockStyle.Left;
						pbx.Image = img;
					}, null);
				}

				
			});

			// Enable new search
			searchButton.Enabled = true;
		}
	}
}
