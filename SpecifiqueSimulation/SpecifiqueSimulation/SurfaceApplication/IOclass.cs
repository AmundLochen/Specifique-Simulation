using SurfaceApplication.Properties;

namespace SurfaceApplication
{
    internal class Oclass
    {
        //Gets the IP Address from the settings file. This class can be used to read other values as needed  from the settings file.
        public static string GetIpAddress()
        {
            string ip = Settings.Default.IP;


            return ip;
        }
    }
}