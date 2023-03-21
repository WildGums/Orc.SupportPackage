﻿namespace Orc.SupportPackage;

using System.IO;
using System.Linq;
using System.IO.Compression;
using System;

/// <summary>
/// Code comes from https://stackoverflow.com/questions/15133626/creating-directories-in-a-ziparchive-c-sharp-net-4-5
/// </summary>
public static class ZipArchiveExtensions
{
    public static void CreateEntryFromAny(this ZipArchive archive, string sourceName, string entryName, CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(sourceName);
        ArgumentNullException.ThrowIfNull(entryName);

        if (File.GetAttributes(sourceName).HasFlag(FileAttributes.Directory))
        {
            archive.CreateEntryFromDirectory(sourceName, entryName, compressionLevel);
        }
        else
        {
            archive.CreateEntryFromFile(sourceName, entryName, compressionLevel);
        }
    }

    public static void CreateEntryFromDirectory(this ZipArchive archive, string sourceDirName, string entryName, CompressionLevel compressionLevel)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(sourceDirName);
        ArgumentNullException.ThrowIfNull(entryName);

        var files = Directory.EnumerateFileSystemEntries(sourceDirName);
        if (files.Any())
        {
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                archive.CreateEntryFromAny(file, Path.Combine(entryName, fileName), compressionLevel);
            }
        }
        else
        {
            //Do a folder entry check.
            if (!string.IsNullOrEmpty(entryName) && entryName[^1] != '/')
            {
                entryName += "/";
            }

            archive.CreateEntry(entryName, compressionLevel);
        }
    }
}
