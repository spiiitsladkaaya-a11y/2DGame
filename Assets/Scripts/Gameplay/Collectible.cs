using NeonDrift.Core;

namespace NeonDrift.Gameplay
{
    public sealed class Collectible : SpawnedObject
    {
        public void Collect()
        {
            PopAndDestroy(GamePalette.Gold);
        }
    }
}
