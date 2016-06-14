using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;

namespace UWPHelpers
{
    public class MusicPropertiesWithPath
    {
        public MusicProperties DefinedProperties { get; set; }

        public string FileName { get; set; }

    }

    /// <summary>
    /// 本地音乐的帮助类
    /// </summary>
    public static class LocalMusicHelper
    {
        /// <summary>
        /// 扫描本地Music Library的所有音乐
        /// </summary>
        /// <returns>返回所有的音乐文件属性和相对music library的路径</returns>
        public static async Task<List<MusicPropertiesWithPath>> ScanLocalMusicInLibrary()
        {
            StorageFolder folder = KnownFolders.MusicLibrary;

            List<MusicPropertiesWithPath> songs = new List<MusicPropertiesWithPath>();

            StorageFileQueryResult query = folder.CreateFileQuery(CommonFileQuery.OrderByMusicProperties);

            //var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByMusicProperties);
            var files = await query.GetFilesAsync();

            foreach (var file in files)
            {
                var properties = await file.Properties.GetMusicPropertiesAsync();
                try
                {
                    using (var stream = await file.OpenReadAsync())
                    {
                        songs.Add(new MusicPropertiesWithPath()
                        {
                            DefinedProperties = properties,
                            FileName = file.Path.Substring(file.Path.IndexOf("Music")).Remove(0, 6)
                        });

                    }
                }
                catch (Exception ex)
                {

                }

            }

            return songs;
        }

    }
}
