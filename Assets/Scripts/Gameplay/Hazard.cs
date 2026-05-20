using NeonDrift.Core;

namespace NeonDrift.Gameplay
{
    public sealed class Hazard : SpawnedObject
    {
        public void Impact()
        {
            PopAndDestroy(GamePalette.Coral);
        }
    }
}
