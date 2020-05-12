using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SvnManager.WebUI
{
    public class SvnBackups
    {
        private static string AccessKey => ConfigurationManager.AppSettings["Backups.S3AccessKey"];
        private static string SecretKey => ConfigurationManager.AppSettings["Backups.S3SecretKey"];
        private static string BucketName => ConfigurationManager.AppSettings["Backups.S3BucketName"];
        private static string BackupLocation => ConfigurationManager.AppSettings["Backups.BackupLocation"];
        private static string RepoPath => ConfigurationManager.AppSettings["Manager.RepoPath"];
        private static string SvnLocation => ConfigurationManager.AppSettings["SvnLocation"];
        private static int DaysBetweenBackups => Convert.ToInt32(ConfigurationManager.AppSettings["Backups.DaysBetweenBackups"]) * -1;

        public static void Upload()
        {
            // For debugging with Fiddler
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            try
            {
                BasicAWSCredentials cred = new BasicAWSCredentials(AccessKey, SecretKey);
                AmazonS3Client c = new AmazonS3Client(cred, Amazon.RegionEndpoint.USWest2);
                var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc > DateTime.UtcNow.AddDays(DaysBetweenBackups));
                foreach (var file in files)
                {
                    InitiateMultipartUploadResponse initResponse = c.InitiateMultipartUpload(new InitiateMultipartUploadRequest { StorageClass = S3StorageClass.StandardInfrequentAccess, BucketName = BucketName, Key = file.Name });
                    long partSize = Convert.ToInt64(ConfigurationManager.AppSettings["Backups.S3PartSize"]) * (long)Math.Pow(2, 20);
                    List<UploadPartResponse> resps = new List<UploadPartResponse>();

                    try
                    {
                        long filePosition = 0;
                        for (int i = 1; filePosition < file.Length; i++)
                        {
                            UploadPartRequest upReq = new UploadPartRequest
                            {
                                BucketName = BucketName,
                                Key = file.Name,
                                UploadId = initResponse.UploadId,
                                PartNumber = i,
                                PartSize = partSize,
                                FilePosition = filePosition,
                                FilePath = file.FullName
                            };

                            int tries = 1;
                            UPLOAD:
                            try
                            {
                                resps.Add(c.UploadPart(upReq));
                            }
                            catch (Exception ex)
                            {
                                var origEx = ex;
                                while (ex.InnerException != null)
                                    ex = ex.InnerException;

                                if (ex is System.Net.Sockets.SocketException && ex.Message.Contains("connection attempt failed") && tries <= 3)
                                {
                                    tries++;
                                    Thread.Sleep(30_000);
                                    goto UPLOAD;
                                }
                                throw origEx;
                            }

                            filePosition += partSize;
                        }

                        CompleteMultipartUploadRequest completeReq = new CompleteMultipartUploadRequest
                        {
                            BucketName = BucketName,
                            Key = file.Name,
                            UploadId = initResponse.UploadId
                        };
                        completeReq.AddPartETags(resps);
                        var completeResp = c.CompleteMultipartUpload(completeReq);
                    }
                    catch (Exception ex)
                    {
                        AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                        {
                            BucketName = BucketName,
                            Key = file.Name,
                            UploadId = initResponse.UploadId
                        };
                        c.AbortMultipartUpload(abortMPURequest);
                        Notification.SendError(ex, file.Name);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Notification.SendError(ex);
            }
        }

        public static void DeleteLocal()
        {
            try
            {
                var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc < DateTime.UtcNow.AddDays(DaysBetweenBackups));
                foreach (var file in files)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Notification.SendError(ex);
            }
        }

        public static void RunBackups()
        {
            string output;
            try
            {
                var repos = new DirectoryInfo(RepoPath).GetDirectories();
                foreach (var r in repos)
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo = new ProcessStartInfo($@"{SvnLocation}\svnadmin", $@"dump {r.FullName} > {BackupLocation}\{r.Name}_{DateTime.UtcNow:yyyyMMddHHmm}.dump")
                        {
                            RedirectStandardError = true
                        };
                        p.Start();
                        p.WaitForExit();
                        output = p.StandardError.ReadToEnd();
                        if (p.ExitCode != 0)
                        {
                            // TODO: Send email with output ?
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Notification.SendError(ex);
            }
        }

        public static bool IsBackupTime()
        {
            var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc > DateTime.UtcNow.AddDays(-DaysBetweenBackups));
            if (files.Count() <= 0 && DateTime.UtcNow.Hour > 3 && DateTime.UtcNow.Hour < 8)
                return true;

            return false;
        }

        public static bool IsBackingUp { get; set; }
    }
}
