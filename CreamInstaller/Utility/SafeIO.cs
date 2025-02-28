﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CreamInstaller.Forms;

namespace CreamInstaller.Utility;

internal static class SafeIO
{
    internal static bool FileLocked(this string filePath)
    {
        if (!FileExists(filePath))
            return false;
        try
        {
            File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None).Close();
        }
        catch
        {
            return true;
        }
        return false;
    }

    internal static bool DirectoryExists(this string directoryPath) => Directory.Exists(directoryPath);

    internal static void CreateDirectory(this string directoryPath, bool crucial = false, Form form = null)
    {
        if (directoryPath.DirectoryExists())
            return;
        while (!Program.Canceled)
            try
            {
                Directory.CreateDirectory(directoryPath);
                break;
            }
            catch
            {
                if (!crucial || directoryPath.DirectoryExists() || directoryPath.IOWarn("Failed to create a crucial directory", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static void MoveDirectory(this string directoryPath, string newDirectoryPath, bool crucial = false, Form form = null)
    {
        if (!directoryPath.DirectoryExists())
            return;
        while (!Program.Canceled)
            try
            {
                Directory.Move(directoryPath, newDirectoryPath);
                break;
            }
            catch
            {
                if (!crucial || !directoryPath.DirectoryExists() || directoryPath.IOWarn("Failed to move a crucial directory", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static void DeleteDirectory(this string directoryPath, bool crucial = false, Form form = null)
    {
        if (!directoryPath.DirectoryExists())
            return;
        while (!Program.Canceled)
            try
            {
                Directory.Delete(directoryPath, true);
                break;
            }
            catch
            {
                if (!crucial || !directoryPath.DirectoryExists() || directoryPath.IOWarn("Failed to delete a crucial directory", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static IEnumerable<string> EnumerateDirectory(this string directoryPath, string filePattern, bool subdirectories = false, bool crucial = false,
        Form form = null)
    {
        if (!directoryPath.DirectoryExists())
            return Enumerable.Empty<string>();
        while (!Program.Canceled)
            try
            {
                return subdirectories
                    ? Directory.EnumerateFiles(directoryPath, filePattern, new EnumerationOptions { RecurseSubdirectories = true })
                    : Directory.EnumerateFiles(directoryPath, filePattern);
            }
            catch
            {
                if (!crucial || !directoryPath.DirectoryExists()
                             || directoryPath.IOWarn("Failed to enumerate a crucial directory's files", form) is not DialogResult.OK)
                    break;
            }
        return Enumerable.Empty<string>();
    }

    internal static IEnumerable<string> EnumerateSubdirectories(this string directoryPath, string directoryPattern, bool subdirectories = false,
        bool crucial = false, Form form = null)
    {
        if (!directoryPath.DirectoryExists())
            return Enumerable.Empty<string>();
        while (!Program.Canceled)
            try
            {
                return subdirectories
                    ? Directory.EnumerateDirectories(directoryPath, directoryPattern, new EnumerationOptions { RecurseSubdirectories = true })
                    : Directory.EnumerateDirectories(directoryPath, directoryPattern);
            }
            catch
            {
                if (!crucial || !directoryPath.DirectoryExists()
                             || directoryPath.IOWarn("Failed to enumerate a crucial directory's subdirectories", form) is not DialogResult.OK)
                    break;
            }
        return Enumerable.Empty<string>();
    }

    internal static bool FileExists(this string filePath) => File.Exists(filePath);

    internal static void CreateFile(this string filePath, bool crucial = false, Form form = null)
    {
        while (!Program.Canceled)
            try
            {
                File.Create(filePath).Close();
                break;
            }
            catch
            {
                if (!crucial || filePath.IOWarn("Failed to create a crucial file", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static void MoveFile(this string filePath, string newFilePath, bool crucial = false, Form form = null)
    {
        if (!filePath.FileExists())
            return;
        while (!Program.Canceled)
            try
            {
                File.Move(filePath, newFilePath);
                break;
            }
            catch
            {
                if (!crucial || !filePath.FileExists() || filePath.IOWarn("Failed to move a crucial file", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static void DeleteFile(this string filePath, bool crucial = false, Form form = null)
    {
        if (!filePath.FileExists())
            return;
        while (!Program.Canceled)
            try
            {
                File.Delete(filePath);
                break;
            }
            catch
            {
                if (!crucial || !filePath.FileExists() || filePath.IOWarn("Failed to delete a crucial file", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static string ReadFile(this string filePath, bool crucial = false, Form form = null)
    {
        if (!filePath.FileExists())
            return null;
        while (!Program.Canceled)
            try
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch
            {
                if (!crucial || !filePath.FileExists() || filePath.IOWarn("Failed to read a crucial file", form) is not DialogResult.OK)
                    break;
            }
        return null;
    }

    internal static byte[] ReadFileBytes(this string filePath, bool crucial = false, Form form = null)
    {
        if (!filePath.FileExists())
            return null;
        while (!Program.Canceled)
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch
            {
                if (!crucial || !filePath.FileExists() || filePath.IOWarn("Failed to read a crucial file", form) is not DialogResult.OK)
                    break;
            }
        return null;
    }

    internal static void WriteFile(this string filePath, string text, bool crucial = false, Form form = null)
    {
        while (!Program.Canceled)
            try
            {
                File.WriteAllText(filePath, text, Encoding.UTF8);
                break;
            }
            catch
            {
                if (!crucial || filePath.IOWarn("Failed to write a crucial file", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static void ExtractZip(this string archivePath, string destinationPath, bool crucial = false, Form form = null)
    {
        if (!archivePath.FileExists())
            return;
        while (!Program.Canceled)
            try
            {
                ZipFile.ExtractToDirectory(archivePath, destinationPath);
                break;
            }
            catch
            {
                if (!crucial || !archivePath.FileExists() || archivePath.IOWarn("Failed to extract a crucial zip file", form) is not DialogResult.OK)
                    break;
            }
    }

    internal static DialogResult IOWarn(this string filePath, string message, Form form = null)
    {
        using DialogForm dialogForm = new(form ?? Form.ActiveForm);
        return dialogForm.Show(SystemIcons.Warning, message + ": " + filePath.BeautifyPath(), "Retry", "OK");
    }
}