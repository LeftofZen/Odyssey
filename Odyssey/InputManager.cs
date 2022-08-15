using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey
{
	public class InputManager : GameComponent
	{
		public InputManager(Game game) : base(game)
		{
		}

		public new void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//prevKeysDown = keysDown;

			//var mouse = Mouse.GetState();
			//var keyboard = Keyboard.GetState(PlayerIndex.One);

			//keysDown = keyboard.GetPressedKeys();
			//if ()
		}
	}
}
