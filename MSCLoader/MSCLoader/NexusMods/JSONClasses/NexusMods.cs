using System;
using System.Collections.Generic;

namespace MSCLoader.NexusMods.JSONClasses.NexusMods
{
    class ModInfo
    {
        public string name { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public string picture_url { get; set; }
        public long uid { get; set; }
        public int mod_id { get; set; }
        public int game_id { get; set; }
        public bool allow_rating { get; set; }
        public string domain_name { get; set; }
        public int category_id { get; set; }
        public string version { get; set; }
        public int endorsement_count { get; set; }
        public int created_timestamp { get; set; }
        public DateTime created_time { get; set; }
        public int updated_timestamp { get; set; }
        public DateTime updated_time { get; set; }
        public string author { get; set; }
        public string uploaded_by { get; set; }
        public string uploaded_users_profile_url { get; set; }
        public bool contains_adult_content { get; set; }
        public string status { get; set; }
        public bool available { get; set; }
        public User user { get; set; }
        public Endorsement endorsement { get; set; }
    }

    public class ModFiles
    {
        public List<File> files { get; set; }
        public List<FileUpdate> file_updates { get; set; }
    }

    public class DownloadSources
    {
        public string name { get; set; }
        public string short_name { get; set; }
        public string URI { get; set; }
    }

    #region ModInfo
    class User
    {
        public int member_id { get; set; }
        public int member_group_id { get; set; }
        public string name { get; set; }
    }

    class Endorsement
    {
        public string endorse_status { get; set; }
        public object timestamp { get; set; }
        public object version { get; set; }
    }
    #endregion
    #region ModFiles
    public class File
    {
        public List<int> id { get; set; }
        public object uid { get; set; }
        public int file_id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public int category_id { get; set; }
        public string category_name { get; set; }
        public bool is_primary { get; set; }
        public int size { get; set; }
        public string file_name { get; set; }
        public int uploaded_timestamp { get; set; }
        public DateTime uploaded_time { get; set; }
        public string mod_version { get; set; }
        public string external_virus_scan_url { get; set; }
        public string description { get; set; }
        public int size_kb { get; set; }
        public string changelog_html { get; set; }
        public string content_preview_link { get; set; }
    }

    public class FileUpdate
    {
        public int old_file_id { get; set; }
        public int new_file_id { get; set; }
        public string old_file_name { get; set; }
        public string new_file_name { get; set; }
        public int uploaded_timestamp { get; set; }
        public DateTime uploaded_time { get; set; }
    }
    #endregion
}
