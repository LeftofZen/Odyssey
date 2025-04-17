using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Odyssey
{
	// I know XNA/Monogame has some kind of GameServices like this with components, need to look that up
	// to implement this pattern correctly and avoid rewriting code
	public static class GameServices
	{
		public static readonly Dictionary<string, Texture2D> Textures = []; // TextureCollection??
		public static readonly Dictionary<string, SpriteFont> Fonts = []; //
		public static readonly Dictionary<string, SoundEffect> SoundEffects = []; // SoundBank??
		public static readonly Dictionary<string, Song> Songs = []; // SongCollection??
		public static InputManager InputManager;
		public static readonly Random PRNG = new(Constants.Seed);

		public static void LoadContent(ContentManager Content, GraphicsDevice graphicsDevice)
		{
			// TODO: use this.Content to load your game content here
			//var texNames = new List<string> { "terrain", "char", "ui", "animals", "grassland" };
			//foreach (var v in texNames)
			//{
			//	Textures.Add(v, Content.Load<Texture2D>("textures\\" + v));
			//}

			Textures.Add("grass", Generation.Textures.GenGrassTexture(graphicsDevice, 32, 32));

			var pixel = new Texture2D(graphicsDevice, 1, 1);
			pixel.SetData(new Color[] { Color.White });
			Textures.Add(nameof(pixel), pixel);

			Content.RootDirectory = "C:\\Users\\bigba\\source\\repos\\Odyssey\\Content\\Content\\bin\\DesktopGL";
			var fontNames = new List<string> { "Calibri" };
			foreach (var v in fontNames)
			{
				Fonts.Add(v, Content.Load<SpriteFont>("Fonts\\" + v));
				//Fonts.Add(v, null);
			}

			//var songNames = new List<string> { "farm_music" };
			//foreach (var v in songNames)
			//{
			//	Songs.Add(v, Content.Load<Song>("songs\\" + v));
			//}

			//var sfxNames = new List<string> { "dogbark", "dog2", "ponywhinny", "rooster" };
			//foreach (var v in sfxNames)
			//{
			//	SoundEffects.Add(v, Content.Load<SoundEffect>("soundeffects\\" + v));
			//}

			//MediaPlayer.Play(GameServices.Songs["farm_music"]);
		}
	}
}
