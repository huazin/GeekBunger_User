using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Fiap.GeekBurguer.Users.Contract;
using System.IO;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Fiap.GeekBurguer.Users.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        #region MOCK

        private Guid UserIdUm;
        private Guid UserIdDois;
        private string faceUm;
        private string faceDois;

        private List<FoodRestrictions> listaRestricoes;

        #endregion
        #region Face Properties
        private readonly IConfiguration configuration;
        private static FaceServiceClient faceServiceClient;
        private static Guid FaceListId;
        #endregion

        public UsersController(IConfiguration configuration)
        {
            UserIdUm = new Guid("31914279-0fec-44a3-864e-70a31c8bf832");
            UserIdDois = new Guid("5673d5ce-2284-4786-984d-a8d69b2fd8a4");

            faceUm = "ImagemUm";
            faceDois = "ImagemDois";

            //listaRestricoes = new List<FoodRestrictions>();

            //listaRestricoes.Add(
            //    new FoodRestrictions
            //    {
            //        RequesterId = Guid.NewGuid(),
            //        UserId = UserIdUm,
            //        Restrictions = new List<string>() { "soja", "gluten" },
            //        Others = "brocolis"
            //    }
            //);
            //listaRestricoes.Add(
            //    new FoodRestrictions
            //    {
            //        RequesterId = Guid.NewGuid(),
            //        UserId = UserIdDois,
            //        Restrictions = new List<string>() { "lactose" },
            //        Others = "ovos"
            //    }
            //);
            faceServiceClient = new FaceServiceClient(configuration["KeyFaceDetectionAPI"], configuration["UrlFaceDetectionApi"]);
            this.configuration = configuration;
        }


        [HttpPost("EnviaFace")]
        public IActionResult GetUserByFace([FromBody]User user)
        {
            FaceListId = Guid.Parse(configuration["FaceListId"]);

            while (true)
            {
                try
                {
                    var containsAnyFaceOnList = UpsertFaceListAndCheckIfContainsFaceAsync().Result;

                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), "Faces");

                    string imageName = Guid.NewGuid().ToString() + ".jpg";

                    //set the image path
                    string imgPath = Path.Combine(pathToSave, imageName);
                    System.IO.File.WriteAllBytes(imgPath, user.Face);
                    
                    //MemoryStream imageStream = new MemoryStream(user.Face);


                    var face = DetectFaceAsync(imgPath).Result;
                    if (face != null)
                    {
                        Guid? persistedId = null;
                        if (containsAnyFaceOnList)
                            persistedId = FindSimilarAsync(face.FaceId, FaceListId).Result;

                        if (persistedId == null)
                            persistedId = AddFaceAsync(FaceListId, imgPath, face.FaceRectangle).Result;

                        user.UserId = (Guid)persistedId;

                        System.IO.File.Delete(imgPath);

                        return Ok(user);
                    }
                    else
                    {
                        return BadRequest("Imagem não é uma face!");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Ocorreu um erro durante o reconhecimento facial!");
                }
            }
        }

        [HttpGet("{userId}/GetFoodRestrictionsByUserId")]
        public IActionResult GetFoodRestrictionsByUserId(Guid userId)
        {
            FoodRestrictions restricao = listaRestricoes.Find(l => l.UserId == userId);

            return Ok(restricao);
        }

        [HttpPost]
        public IActionResult PostFoodRestrictionsByUserId(FoodRestrictions foodRestrictions)
        {
            if (foodRestrictions.UserId != Guid.Empty)
            {
                return Ok();
            }

            return new NotFoundResult();
        }

        #region Api Face Detection
        private static async Task<bool> UpsertFaceListAndCheckIfContainsFaceAsync()
        {
            var faceListId = FaceListId.ToString();
            var faceLists = await faceServiceClient.ListFaceListsAsync();
            var faceList = faceLists.FirstOrDefault(_ => _.FaceListId == FaceListId.ToString());

            if (faceList == null)
            {
                await faceServiceClient.CreateFaceListAsync(faceListId, "GeekBurgerFaces", null);
                return false;
            }

            var faceListJustCreated = await faceServiceClient.GetFaceListAsync(faceListId);

            return faceListJustCreated.PersistedFaces.Any();
        }

        private static async Task<Guid?> FindSimilarAsync(Guid faceId, Guid faceListId)
        {
            var similarFaces = await faceServiceClient.FindSimilarAsync(faceId, faceListId.ToString());

            var similarFace = similarFaces.FirstOrDefault(_ => _.Confidence > 0.5);

            return similarFace?.PersistedFaceId;
        }

        private static async Task<Face> DetectFaceAsync(string imageUrl)
        {
            try
            {
                using (Stream imageFileStream = System.IO.File.OpenRead(imageUrl))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    return faces.FirstOrDefault();
                }

                //var faces = await faceServiceClient.DetectAsync(imageStream);
                //return faces.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Guid?> AddFaceAsync(Guid faceListId, string imageFilePath, FaceRectangle rectangle)
        {
            //try
            //{
            //    AddPersistedFaceResult faceResult;
            //    faceResult = await faceServiceClient.AddFaceToFaceListAsync(faceListId.ToString(), imageStream, null, rectangle);
            //    return faceResult.PersistedFaceId;
            //}
            try
            {
                AddPersistedFaceResult faceResult;
                using (Stream imageFileStream = System.IO.File.OpenRead(imageFilePath))
                {
                    faceResult = await faceServiceClient.AddFaceToFaceListAsync(faceListId.ToString(), imageFileStream, null, rectangle);
                    return faceResult.PersistedFaceId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Face não incluída na lista!");
                return null;
            }
        }
        #endregion
    }

}