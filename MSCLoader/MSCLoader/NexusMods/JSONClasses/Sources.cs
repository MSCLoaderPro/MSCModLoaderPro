
#pragma warning disable IDE1006
namespace MSCLoader.NexusMods.JSONClasses.ProLoader
{
    class Sources
    {
        public string id { get; set; }
        public string url { get; set; }
        public string min_ver { get; set; }
        public Dependency[] dependencies { get; set; }
    }

    class Dependency
    { 
        public string id { get; set; }
        public string url { get; set; }
    }
}
