using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SvnManager.Api
{
    public class SvnBackups
    {
        private const string AccessKey = "";
        private const string SecretKey = "";
        private const string BucketName = "";
        private const string BackupLocation = "";

        public static void Upload()
        {
            // For debugging with Fiddler
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            try
            {
                BasicAWSCredentials cred = new BasicAWSCredentials(AccessKey, SecretKey);
                AmazonS3Client c = new AmazonS3Client(cred, Amazon.RegionEndpoint.USWest2);
                var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc > DateTime.UtcNow.AddDays(-7));
                foreach (var file in files)
                {
                    InitiateMultipartUploadResponse initResponse = c.InitiateMultipartUpload(new InitiateMultipartUploadRequest { StorageClass = S3StorageClass.StandardInfrequentAccess, BucketName = BucketName, Key = file.Name });
                    long partSize = 100 * (long)Math.Pow(2, 20);
                    List<UploadPartResponse> resps = new List<UploadPartResponse>();
                    //var resp = c.PutObject(new Amazon.S3.Model.PutObjectRequest() { FilePath = file.FullName, BucketName = BucketName, Key = file.Name, StorageClass = S3StorageClass.StandardInfrequentAccess });
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

                            resps.Add(c.UploadPart(upReq));

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
                var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc < DateTime.UtcNow.AddDays(-7));
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
            try
            {
                // TODO: Create backups
                //using (PowerShell ps = PowerShell.Create())
                //{
                //    ps.AddCommand("Backup-SvnRepository");
                //    ps.AddArgument("*");
                //    ps.Invoke();
                //}
            }
            catch (Exception ex)
            {
                Notification.SendError(ex);
            }
        }

        public static bool IsBackupTime()
        {
            var files = new DirectoryInfo(BackupLocation).EnumerateFiles().Where(f => f.CreationTimeUtc > DateTime.UtcNow.AddDays(-7));
            if (files.Count() <= 0 && DateTime.UtcNow.Hour > 3 && DateTime.UtcNow.Hour < 8)
                return true;

            return false;
        }

        public static bool IsBackingUp { get; set; }
    }
}
