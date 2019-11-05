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

namespace Fiap.GeekBurguer.Users.Controller
{
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        #region MOCK

        private Guid UserIdUm;
        private Guid UserIdDois;
        public static FaceServiceClient faceServiceClient;
        public static Guid FaceListId;
        private string faceUm;
        private string faceDois;

        private List<FoodRestrictions> listaRestricoes;

        #endregion

        public UsersController()
        {
            UserIdUm = new Guid("31914279-0fec-44a3-864e-70a31c8bf832");
            UserIdDois = new Guid("5673d5ce-2284-4786-984d-a8d69b2fd8a4");

            faceUm = "ImagemUm";
            faceDois = "ImagemDois";

            listaRestricoes = new List<FoodRestrictions>();

            listaRestricoes.Add(
                new FoodRestrictions {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdUm,
                    Restrictions = "['soja','gluten']",
                    Others = "brocolis"
                }
            );
            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdDois,
                    Restrictions = "['lactose']",
                    Others = "ovos"
                }
            );
        }    


        [HttpGet]
        public IActionResult GetUserByFace(User user)
        {
            if (user.Face == "ImagemUm")
            {
                user.UserId = UserIdUm;
            }
            else if (user.Face == "ImagemDois")
            {
                user.UserId = UserIdDois;
            }

            return Ok(user);
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

        private static async Task<Face> DetectFaceAsync(string imageFilePath)
        {
            try
            {
                using (Stream imageFileStream = System.IO.File.OpenRead(imageFilePath))
                {
                    var faces = await faceServiceClient.DetectAsync(imageFileStream);
                    return faces.FirstOrDefault();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Guid?> AddFaceAsync(Guid faceListId, string imageFilePath)
        {
            try
            {
                AddPersistedFaceResult faceResult;
                using (Stream imageFileStream = System.IO.File.OpenRead(imageFilePath))
                {
                    faceResult = await faceServiceClient.AddFaceToFaceListAsync(faceListId.ToString(), imageFileStream);
                    return faceResult.PersistedFaceId;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Face not included in Face List!");
                return null;
            }
        }
    }
}