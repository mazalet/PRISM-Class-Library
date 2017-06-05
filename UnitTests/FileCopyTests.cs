﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using PRISM;

namespace PRISMTest
{
    [TestFixture]
    class FileCopyTests
    {
        private clsFileTools mFileTools;

        [OneTimeSetUp]
        public void Setup()
        {
            mFileTools = new clsFileTools();
        }

        [TestCase(@"C:\Temp\PRISM", @"C:\Temp\PRISMCopy")]
        public void CopyDirectoryLocal(string sourceFolderPath, string targetFolderPath)
        {
            var sourceFolder = new DirectoryInfo(sourceFolderPath);
            if (!sourceFolder.Exists)
            {
                try
                {
                    sourceFolder.Create();

                    var rand = new Random();

                    for (var i = 1; i <= 4; i++)
                    {
                        var testFilePath = Path.Combine(sourceFolder.FullName, "TestFile" + i + ".txt");

                        using (var testFile = new StreamWriter(new FileStream(testFilePath, FileMode.Create, FileAccess.Write)))
                        {
                            testFile.WriteLine("X\tY");
                            for (var j = 1; j <= 10000 * i; j++)
                            {
                                testFile.WriteLine("{0}\t{1}", j, rand.Next(0, 1000));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail("Error creating test folder and/or test files at " + sourceFolder + ": " + ex.Message);
                }

            }

            CopyDirectory(sourceFolderPath, targetFolderPath);
        }

        [TestCase(@"\\proto-2\UnitTest_Files\PRISM", @"\\proto-2\UnitTest_Files\PRISM\FolderCopyTest")]
        [Category("PNL_Domain")]
        public void CopyDirectoryRemote(string sourceFolderPath, string targetFolderPath)
        {
            CopyDirectory(sourceFolderPath, targetFolderPath);
        }

        [TestCase(@"\\proto-2\UnitTest_Files\PRISM", @"\\proto-2\UnitTest_Files\PRISM\FolderCopyTest")]
        [Category("PNL_Domain")]
        public void CopyDirectory(string sourceFolderPath, string targetFolderPath)
        {
            var sourceFolder = new DirectoryInfo(sourceFolderPath);
            if (!sourceFolder.Exists)
            {
                Assert.Fail("Source directory not found: " + sourceFolderPath);
            }

            var filesToSkip = new List<string> { "H_sapiens_Uniprot_trembl_2015-10-14.fasta" };

            var targetFolder = new DirectoryInfo(targetFolderPath);
            if (targetFolder.Exists)
                targetFolder.Delete(true);

            mFileTools.CopyDirectory(sourceFolderPath, targetFolderPath, true, filesToSkip);
        }

        [TestCase(@"C:\Windows\win.ini", @"C:\temp\win.ini")]
        public void CopyFileLocal(string sourceFilePath, string targetFilePath)
        {
            CopyFile(sourceFilePath, targetFilePath);
        }

        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\Tryp_Pig_Bov.fasta", @"\\proto-2\UnitTest_Files\PRISM\FileCopyTest\Tryp_Pig_Bov.fasta")]
        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\HumanContam.fasta", @"\\proto-2\UnitTest_Files\PRISM\FileCopyTest\HumanContam_Renamed.fasta")]
        [Category("PNL_Domain")]
        public void CopyFileRemote(string sourceFilePath, string targetFilePath)
        {
            CopyFile(sourceFilePath, targetFilePath);
        }

        private void CopyFile(string sourceFilePath, string targetFilePath)
        {
            var sourceFile = new FileInfo(sourceFilePath);
            if (!sourceFile.Exists)
            {
                Assert.Fail("Source file not found: " + sourceFile);
            }

            var targetFile = new FileInfo(targetFilePath);
            if (targetFile.Exists)
                targetFile.Delete();

            mFileTools.CopyFile(sourceFilePath, targetFilePath, false);

            System.Threading.Thread.Sleep(150);
            mFileTools.CopyFile(sourceFilePath, targetFilePath, true);

            System.Threading.Thread.Sleep(150);

            // Copy the file again, but with overwrite = false
            // This should raise an exception
            bool exceptionRaised;

            try
            {
                mFileTools.CopyFile(sourceFilePath, targetFilePath, false);
                exceptionRaised = false;
            }
            catch (Exception)
            {
                exceptionRaised = true;
            }

            Assert.IsTrue(exceptionRaised, "File copy with overwrite = false did not raise an exception; it should have");



        }

        [TestCase(@"C:\Windows\win.ini", @"C:\temp\win.ini")]
        public void CopyFileUsingLocksLocal(string sourceFilePath, string targetFilePath)
        {
            CopyFileUsingLocks(sourceFilePath, targetFilePath);
        }

        [TestCase(@"\\gigasax\DMS_Organism_Files\Homo_sapiens\Fasta\H_sapiens_Uniprot_trembl_2015-10-14.fasta",
            @"\\proto-2\UnitTest_Files\PRISM\FileCopyTestWithLocks\H_sapiens_Uniprot_trembl_2015-10-14.fasta")]
        [TestCase(@"\\gigasax\DMS_Organism_Files\Homo_sapiens\Fasta\H_sapiens_Uniprot_SPROT_2015-04-22.fasta",
            @"\\proto-2\UnitTest_Files\PRISM\FileCopyTestWithLocks\H_sapiens_Uniprot_SPROT_2015-04-22.fasta")]
        [Category("PNL_Domain")]
        public void CopyFileUsingLocksRemote(string sourceFilePath, string targetFilePath)
        {
            CopyFileUsingLocks(sourceFilePath, targetFilePath);
        }

        private void CopyFileUsingLocks(string sourceFilePath, string targetFilePath)
        {
            var sourceFile = new FileInfo(sourceFilePath);
            if (!sourceFile.Exists)
            {
                Assert.Fail("Source file not found: " + sourceFile);
            }

            mFileTools.CopyFileUsingLocks(sourceFilePath, targetFilePath, "PrismUnitTests", true);

            System.Threading.Thread.Sleep(150);
            mFileTools.CopyFileUsingLocks(sourceFilePath, targetFilePath, "PrismUnitTests", false);

            System.Threading.Thread.Sleep(150);

            // Copy the file again, but with overwrite = false
            // This should raise an exception
            bool exceptionRaised;

            try
            {
                mFileTools.CopyFile(sourceFilePath, targetFilePath, false);
                exceptionRaised = false;
            }
            catch (Exception)
            {
                exceptionRaised = true;
            }

            Assert.IsTrue(exceptionRaised, "File copy with overwrite = false did not raise an exception; it should have");

        }

        [TestCase(@"C:\Temp")]
        [TestCase(@"C:\Temp\")]
        public void GetDriveFreeSpaceForDirectoryLocal(string directoryPath)
        {
            GetDriveFreeSpaceForDirectory(directoryPath);
        }

        [TestCase(@"\\proto-2\UnitTest_Files")]
        [TestCase(@"\\proto-2\UnitTest_Files\")]
        [TestCase(@"\\protoapps\UserData\Matt\")]
        [Category("PNL_Domain")]
        public void GetDriveFreeSpaceForDirectoryRemote(string directoryPath)
        {
            GetDriveFreeSpaceForDirectory(directoryPath);
        }

        private void GetDriveFreeSpaceForDirectory(string directoryPath)
        {

            var success = PRISMWin.clsDiskInfo.GetDiskFreeSpace(
                directoryPath,
                out var freeBytesAvailableToUser,
                out var totalDriveCapacityBytes,
                out var totalNumberOfFreeBytes);

            if (!success)
            {
                Assert.Fail("GetDiskFreeSpace reported false for " + directoryPath);
            }

            Console.WriteLine("Free space stats for" + directoryPath);

            Console.WriteLine("{0,-25} {1}", "Free Space", clsFileTools.BytesToHumanReadable(totalNumberOfFreeBytes));
            Console.WriteLine("{0,-25} {1}", "Space available to User", clsFileTools.BytesToHumanReadable(freeBytesAvailableToUser));
            Console.WriteLine("{0,-25} {1}", "Drive Capacity", clsFileTools.BytesToHumanReadable(totalDriveCapacityBytes));

        }

        [TestCase(@"C:\Temp\Testfile.txt", false)]
        public void GetDriveFreeSpaceForFileLocal(string targetFilePath, bool reportFreeSpaceAvailableToUser)
        {
            GetDriveFreeSpaceForFile(targetFilePath, reportFreeSpaceAvailableToUser);
        }

        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\TestFile.txt", false)]
        [TestCase(@"\\protoapps\UserData\Matt\TestFile.txt", false)]
        [TestCase(@"\\protoapps\UserData\Matt\TestFile.txt", true)]
        [Category("PNL_Domain")]
        public void GetDriveFreeSpaceForFileRemote(string targetFilePath, bool reportFreeSpaceAvailableToUser)
        {
            GetDriveFreeSpaceForFile(targetFilePath, reportFreeSpaceAvailableToUser);
        }

        public void GetDriveFreeSpaceForFile(string targetFilePath, bool reportFreeSpaceAvailableToUser)
        {

            var success = PRISMWin.clsDiskInfo.GetDiskFreeSpace(targetFilePath, out var freeSpaceBytes, out var errorMessage, reportFreeSpaceAvailableToUser);

            if (!success)
            {
                Assert.Fail("GetDiskFreeSpace reported false for " + targetFilePath + ": " + errorMessage);
            }

            var directoryPath = new FileInfo(targetFilePath).DirectoryName;

            Console.WriteLine("Free space at {0} is {1} (ReportFreeSpaceAvailableToUse = {2}))",
                directoryPath, clsFileTools.BytesToHumanReadable(freeSpaceBytes), reportFreeSpaceAvailableToUser);

        }

        [TestCase(@"C:\Temp\Testfile.txt", 0)]
        [TestCase(@"C:\Temp\TestHugeFile.raw", 100000)]
        [TestCase(@"C:\Temp\TestHugeFile.raw", 1000000)]
        [TestCase(@"C:\Temp\TestHugeFile.raw", 10000000)]
        public void ValidateFreeDiskSpaceLocal(string targetFilePath, long minimumFreeSpaceMB)
        {
            ValidateFreeDiskSpace(targetFilePath, minimumFreeSpaceMB);
        }

        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\TestFile.txt", 150)]
        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\TestHugeFile.raw", 100000)]
        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\TestHugeFile.raw", 1000000)]
        [TestCase(@"\\proto-2\UnitTest_Files\PRISM\TestHugeFile.raw", 10000000)]
        [TestCase(@"\\protoapps\UserData\Matt\TestFile.txt", 0)]
        [TestCase(@"\\protoapps\UserData\Matt\TestFile.txt", 500)]
        [Category("PNL_Domain")]
        public void ValidateFreeDiskSpaceRemote(string targetFilePath, long minimumFreeSpaceMB)
        {
            ValidateFreeDiskSpace(targetFilePath, minimumFreeSpaceMB);
        }

        public void ValidateFreeDiskSpace(string targetFilePath, long minimumFreeSpaceMB)
        {

            var success = PRISMWin.clsDiskInfo.GetDiskFreeSpace(targetFilePath, out var currentDiskFreeSpaceBytes, out var errorMessage);

            if (!success)
            {
                Assert.Fail("GetDiskFreeSpace reported false for " + targetFilePath + ": " + errorMessage);
            }

            var safeToCopy = clsFileTools.ValidateFreeDiskSpace(targetFilePath, minimumFreeSpaceMB, currentDiskFreeSpaceBytes, out errorMessage);

            var sufficientOrNot = safeToCopy ? "sufficient" : "insufficient";

            Console.WriteLine("Target drive has {0} free space to copy {1} file {2}; {3} free",
                sufficientOrNot,
                clsFileTools.BytesToHumanReadable(minimumFreeSpaceMB * 1024 * 1024),
                targetFilePath,
                clsFileTools.BytesToHumanReadable(currentDiskFreeSpaceBytes));


        }

    }
}
