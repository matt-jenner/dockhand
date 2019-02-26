namespace Dockhand.Utils.Extensions
{
    public static class IntegerExtensions
    {
        public static bool IsValidAsANetworkPort(this int input) => input > 0 && input <= 65535;

    }
}
