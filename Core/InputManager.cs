using Microsoft.Xna.Framework;

namespace Odyssey
{
	public class InputManager : GameComponent
	{
		public InputManager(Game game) : base(game)
		{
		}

		public new void Update(GameTime gameTime) => base.Update(gameTime);//prevKeysDown = keysDown;//var mouse = Mouse.GetState();//var keyboard = Keyboard.GetState(PlayerIndex.One);//keysDown = keyboard.GetPressedKeys();//if ()
	}
}
