﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Google.GData.Photos;
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Extensions.Location;
using Google.Picasa;
using System.Configuration;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using NLog;

namespace PicasaWebSync
{
    class Program
    {
        private static Logger s_logger = LogManager.GetLogger("*");

        static void Main(string[] args)
        {
            string commandLineArgs = string.Join(" ", args);

            if (args.Length == 0 || string.IsNullOrEmpty(args[0]) || commandLineArgs.Contains("-help"))
            {
                Console.WriteLine("PicasaWebSync: A utility to sync local folder photos to Picasa Web Albums.");
                Console.WriteLine("       Author: Brady Holt (http://www.GeekyTidBits.com)");
                Console.WriteLine();
                Console.WriteLine("Usage: picasawebsync.exe folderPath [options]");
                Console.WriteLine();
                Console.WriteLine("Example Usage:");
                Console.WriteLine("   picasawebsync.exe \"C:\\Users\\Public\\Pictures\\My Pictures\\\" -r -v");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("   -u:USERNAME,        Picasa Username (can also be specified in picasawebsync.exe.config)");
                Console.WriteLine("   -p:PASSWORD,        Picasa Password (can also be specified in picasawebsync.exe.config)");
                Console.WriteLine("   -r,                 recursive (include subfolders)");
                Console.WriteLine("   -emptyAlbumFirst,   delete all images in album before adding photos");
                Console.WriteLine("   -addOnly,           add only and do not remove anything from online albums (overrides -emptyAlbumFirst)");
                Console.WriteLine("   -v,                 verbose output");
                Console.WriteLine("   -help,              print this help menu");
                Console.WriteLine();
            }
            else
            {
                s_logger.Info("[Initializing]");
                ServicePointManager.ServerCertificateValidationCallback = CertificateValidator;
                PicasaAlbumSynchronizer uploader = new PicasaAlbumSynchronizer();

                try
                {
                    string includeExtensionsConfig = ConfigurationManager.AppSettings["file.includeExtensions"];
                    string excludeFolderNamesConfig = ConfigurationManager.AppSettings["folder.excludeNames"];
                    string excludeFilesNamesContainingTextConfig = ConfigurationManager.AppSettings["file.excludeWhenFileNameContains"];
                    string privateAccessFolderNamesConfig = ConfigurationManager.AppSettings["album.privateAccess.folderNames"];

                    uploader.PicasaUsername = ConfigurationManager.AppSettings["picasa.username"];
                    if (commandLineArgs.Contains("-u:"))
                    {
                        int startIndex = commandLineArgs.IndexOf("-u:") + 3;
                        uploader.PicasaUsername = commandLineArgs.Substring(startIndex, commandLineArgs.IndexOf(" ", startIndex + 1) - startIndex);
                    }

                    uploader.PicasaPassword = ConfigurationManager.AppSettings["picasa.password"];
                    if (commandLineArgs.Contains("-p:"))
                    {
                        int startIndex = commandLineArgs.IndexOf("-p:") + 3;
                        uploader.PicasaPassword = commandLineArgs.Substring(startIndex, commandLineArgs.IndexOf(" ", startIndex + 1) - startIndex);
                    }

                    uploader.AlbumAccess = (AlbumAccessEnum)Enum.Parse(typeof(AlbumAccessEnum), ConfigurationManager.AppSettings["album.access.default"], true);
                    uploader.IncludeSubFolders = commandLineArgs.Contains("-r");
                    uploader.ClearAlbumPhotosFirst = commandLineArgs.Contains("-emptyAlbumFirst");
                    uploader.IncludeBaseDirectoryInAlbumName = Convert.ToBoolean(ConfigurationManager.AppSettings["album.includeTopDirectoryName"]);
                    uploader.IncludeExtensions = includeExtensionsConfig.Split(',');
                    uploader.ExcludeFileNamesContainingText = excludeFilesNamesContainingTextConfig.Split(',');
                    uploader.ExcludeFilesLargerThan = Convert.ToInt64(ConfigurationManager.AppSettings["file.excludeWhenSizeLargerThan"]);
                    uploader.ExcludeFolderNames = excludeFolderNamesConfig.Split(',');
                    uploader.ExcludeFoldersContainingFileName = ConfigurationManager.AppSettings["folder.exclude.hintFileName"];
                    uploader.ResizePhotos = Convert.ToBoolean(ConfigurationManager.AppSettings["photo.resize"]);
                    uploader.ResizePhotosMaxSize = Convert.ToInt32(ConfigurationManager.AppSettings["photo.resize.maxSize"]);
                    uploader.ResizeVideos = Convert.ToBoolean(ConfigurationManager.AppSettings["video.resize"]);
                    uploader.ResizeVideosCommand = ConfigurationManager.AppSettings["video.resize.command"];
                    uploader.AlbumNameFormat = ConfigurationManager.AppSettings["album.nameFormat"];
                    uploader.AlbumPrivateFileName = ConfigurationManager.AppSettings["album.privateAccess.hintFileName"];
                    if (privateAccessFolderNamesConfig != null && privateAccessFolderNamesConfig.Length > 0)
                    {
                        uploader.AlbumPrivateFolderNames = privateAccessFolderNamesConfig.Split(',');
                    }
                    uploader.AlbumPublicFileName = ConfigurationManager.AppSettings["album.publicAccess.hintFileName"];
                    uploader.AddOnly = commandLineArgs.Contains("-addOnly");
                    uploader.VerboseOutput = commandLineArgs.Contains("-v");

                    uploader.SyncFolder(args[0]);
                   
                }
                catch (Exception ex)
                {
                    s_logger.FatalException("Fatal Error Occured", ex);
                }

                //force flush! (http://nlog-project.org/2011/10/30/using-nlog-with-mono.html)
                LogManager.Configuration = null;
            }
        }

        public static bool CertificateValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //trust all certificates
            return true;
        }


    }
}
