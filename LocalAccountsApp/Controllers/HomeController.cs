using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stripe;
using Stripe.BillingPortal;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cloudmersive.APIClient.NET.ImageRecognition.Api;
using Cloudmersive.APIClient.NET.ImageRecognition.Client;
using Cloudmersive.APIClient.NET.ImageRecognition.Model;
using System.Diagnostics;
using Amazon.Glacier.Transfer;

namespace LocalAccountsApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Upload()
        {

            return View();
        }

        public ActionResult Login()
        {

            return View();
        }

        //Upload & Store Photo + NSFW
        [HttpGet]
        public ActionResult Success(NsfwResult nsfw)
        {
            // Do not actually store your IAM credentials in code. EC2 Role is best
            var awsKey = "AKIAWTXXFJ6NJ6U7KA6E";
            var awsSecretKey = "LtP6Y3eFz7vde+nVAhj7svT9KCgDgE5/AsVs4Cnd";
            var bucketRegion = Amazon.RegionEndpoint.USEast1;   // Your bucket region
            var s3 = new AmazonS3Client(awsKey, awsSecretKey, bucketRegion);
            var putRequest = new PutObjectRequest();

            putRequest.BucketName = "csc-assignment-2-2020";        // Your bucket name
            putRequest.ContentType = "image/jpeg";
            PutObjectResponse putResponse = s3.PutObject(putRequest);

            return View(nsfw);
        }

        [HttpGet]
        public ActionResult Fail(NsfwResult nsfw)
        {
            return View(nsfw);
        }

        //full

        [HttpPost]
        public ActionResult Test1(HttpPostedFileBase ImageFile)
        {

            if (ImageFile == null || ImageFile.ContentLength == 0)
            {
                //Show spinner
                ViewBag.Error = "Please select a file.<br>";
                return View("Index");
            }
            else
            {
                if (ImageFile.FileName.EndsWith("jpg") || ImageFile.FileName.EndsWith("png"))
                {
                    //shaun's code
                    string path = Server.MapPath("~/Doc/" + ImageFile.FileName);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    ImageFile.SaveAs(path);
                    // Configure API key authorization: Apikey
                    Configuration.Default.AddApiKey("Apikey", "1c0df5d9-f2a0-4120-9f58-03e603b6cf53");

                    var apiInstance = new NsfwApi();
                    var imageFile = new System.IO.FileStream(path, System.IO.FileMode.Open); // System.IO.Stream | Image file to perform the operation on.  Common file formats such as PNG, JPEG are supported.
                    System.IO.Stream stream = ImageFile.InputStream;

                    // Do not actually store your IAM credentials in code. EC2 Role is best
                    var awsKey = "AKIAWTXXFJ6NJ6U7KA6E";
                    var awsSecretKey = "LtP6Y3eFz7vde+nVAhj7svT9KCgDgE5/AsVs4Cnd";
                    var bucketRegion = Amazon.RegionEndpoint.USEast1;   // Your bucket region
                    var s3 = new AmazonS3Client(awsKey, awsSecretKey, bucketRegion);
                    var putRequest = new PutObjectRequest();

                    //imageURL
                    string fileName = ImageFile.FileName;
                    string imgURL = "csc-assignment-2-2020.s3-ap-southeast-1.amazonaws.com/" + fileName;

                    //TestDBEntities db = new TestDBEntities();
                    //TalentData data = new TalentData();

                    try
                    {
                        // Describe an image in natural language
                        NsfwResult result = apiInstance.NsfwClassify(imageFile);
                        Debug.WriteLine(result);
                        ViewBag.Score = result.Score;
                        ViewBag.Outcome = result.ClassificationOutcome;
                        //Hide spinner 
                        switch (result.ClassificationOutcome)
                        {
                            case "SafeContent_HighProbability":
                                {
                                    putRequest.BucketName = "csc-assignment-2-2020";        // Your bucket name
                                    putRequest.ContentType = "image/jpeg";
                                    putRequest.InputStream = ImageFile.InputStream;
                                    // key will be the name of the image in your bucket
                                    putRequest.Key = ImageFile.FileName;
                                    PutObjectResponse putResponse = s3.PutObject(putRequest);

                                    //data.talentName = fileName;
                                    //data.imageURL = imgURL;
                                    //db.TalentDatas.Add(data);
                                    //db.SaveChanges();

                                    //List<TalentDataViewModel> dataList = db.TalentDatas.Select(x => new TalentDataViewModel
                                    //{
                                    //    talentName = x.talentName,
                                    //    imageURL = x.imageURL
                                    //}).ToList();

                                    return View("Success", result);
                                }

                            case "UnsafeContent_HighProbability":
                                return View("Fail", result);

                            case "RacyContent":
                                return View("Fail", result);

                            case "SafeContent_ModerateProbability":
                                {
                                    putRequest.BucketName = "csc-assignment-2-2020";        // Your bucket name
                                    putRequest.ContentType = "image/jpeg";
                                    putRequest.InputStream = ImageFile.InputStream;
                                    // key will be the name of the image in your bucket
                                    putRequest.Key = ImageFile.FileName;
                                    PutObjectResponse putResponse = s3.PutObject(putRequest);

                                    return View("Success", result);
                                }

                            default:
                                return View("Index");
                        }


                        return View("Success");
                    }
                    catch (Exception e)
                    {
                        Debug.Print("Exception when calling RecognizeApi.RecognizeDescribe: " + e.Message);
                        return View("Index");
                    }
                }
                else
                {
                    ViewBag.Error = "File type is incorrect.<br>";
                    return View("Index");
                }
            }
        }//end of Upload & Save + NSFW

        [HttpPost]
        public ActionResult Login(FormCollection PC)
        {

            // Set your secret key. Remember to switch to your live secret key in production!
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.ApiKey = "sk_test_51Gub1CD1GhlR3Zf7VQPN0qwG32O9gEEovXXduNah0f6xHHwVlaf9DsuNHzLOpDXgsLvyBCurxjA33xEOUJGTILCZ005ANR5mTY";

            var options = new SessionCreateOptions
            {
                Customer = "cus_HkS69UeMf55BrW",
                ReturnUrl = "https://localhost:44305/Home/Index",
            };

            var service = new SessionService();
            Session test = service.Create(options);

            var config = new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.APSoutheast1 };
            //var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"), Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"));
            var credentials = new BasicAWSCredentials("AKIAWTXXFJ6NF5DQAB7W", "CSoJtJGTFzZ7+AuJ4uJ4nwf9NgS4G7/d8NrtyNbw");
            AmazonDynamoDBClient _dynamoDbClient = new AmazonDynamoDBClient(credentials, config);
            Table table = Table.LoadTable(_dynamoDbClient, "Users");
            var book = new Document();
            book["User_ID"] = test.Id;
            book["customer_ID"] = test.Customer;
            book["object_id"] = test.Id;
            book["object_type"] = test.Object;
            book["return_url"] = test.ReturnUrl;

            table.PutItem(book);

            //var options1 = new WebhookEndpointCreateOptions
            //{
            //    Url = "https://git.heroku.com/sptask6.git",
            //    EnabledEvents = new List<string>
            //{
            //    "charge.failed",
            //    "charge.succeeded",
            //},
            //};
            //var service1 = new WebhookEndpointService();
            //service1.Create(options1);

            //service1.Get("we_1GxplED1GhlR3Zf7y0VwRON8");

            return Redirect(test.Url);
        }//
        [HttpPost]
        public void Glacier(string file)
        {
            string vaultName = "GlacialTest";
            string archiveToUpload = file;
            var credentials = new BasicAWSCredentials("AKIAWTXXFJ6NF5DQAB7W", "CSoJtJGTFzZ7+AuJ4uJ4nwf9NgS4G7/d8NrtyNbw");
            var manager = new ArchiveTransferManager(credentials, Amazon.RegionEndpoint.USEast1);
            string archiveId = manager.Upload(vaultName, "archive description", archiveToUpload).ArchiveId;
            
        }//glacier
        
    }

}
