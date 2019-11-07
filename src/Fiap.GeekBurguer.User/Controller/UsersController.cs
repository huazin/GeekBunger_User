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
using Fiap.GeekBurguer.Persistence.Repository;
using AutoMapper;
using Fiap.GeekBurguer.Users.Contract.Dominios;

namespace Fiap.GeekBurguer.Users.Controller
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
        public static IConfiguration Configuration;
        public static FaceServiceClient faceServiceClient;
        public static Guid FaceListId;

        private FoodRestrictionRepository _repo;
        private readonly IMapper _mapper;
        #endregion

        public UsersController(FoodRestrictionRepository repo, IMapper map)
        {
            _repo = repo;
            _mapper = map;
            UserIdUm = new Guid("31914279-0fec-44a3-864e-70a31c8bf832");
            UserIdDois = new Guid("5673d5ce-2284-4786-984d-a8d69b2fd8a4");

            faceUm = "ImagemUm";
            faceDois = "ImagemDois";

            listaRestricoes = new List<FoodRestrictions>();

            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdUm,
                    Restrictions = new List<Restriction>() { new Restriction("soja"), new Restriction("gluten") },
                    Others = new RestrictionOther("brocolis")
                }
            );
            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdDois,
                    Restrictions = new List<Restriction>() { new Restriction( "lactose" )},
                    Others = new RestrictionOther("ovos")
                }
            );
        }


        [HttpPost]
        public IActionResult GetUserByFace(User user)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            FaceListId = Guid.Parse(Configuration["FaceListId"]);

            faceServiceClient = new FaceServiceClient(Configuration["KeyFaceDetectionAPI"], Configuration["UrlFaceDetectionApi"]);

            while (true)
            {
                try
                {
                    var containsAnyFaceOnList = UpsertFaceListAndCheckIfContainsFaceAsync().Result;
                    MemoryStream imageStream = new MemoryStream(Convert.FromBase64String(user.Face));
                    var face = DetectFaceAsync(imageStream).Result;
                    if (face != null)
                    {
                        Guid? persistedId = null;
                        if (containsAnyFaceOnList)
                            persistedId = FindSimilarAsync(face.FaceId, FaceListId).Result;

                        if (persistedId == null)
                            persistedId = AddFaceAsync(FaceListId, imageStream).Result;

                        user.UserId = (Guid)persistedId;
                        return Ok(user);
                    }
                    else
                    {
                        return BadRequest("Imagem não é uma face!");
                    }
                }
                catch (Exception)
                {
                    return BadRequest("Ocorreu um erro durante o reconhecimento facial!");
                }
            }
        }

        [HttpGet("{userId}/GetFoodRestrictionsByUserId")]
        public IActionResult GetFoodRestrictionsByUserId(Guid userId)
        {
            var foodRestricao = _repo.Obter(userId);
            FoodRestrictions restricao = _mapper.Map<FoodRestrictions>(foodRestricao);

            return Ok(restricao);
        }

        [HttpPost("PostFoodRestrictionsByUserId")]
        public IActionResult PostFoodRestrictionsByUserId(FoodRestrictions foodRestrictions)
        {
            if (foodRestrictions.UserId != Guid.Empty)
            {
                var restrictions = _mapper.Map<Fiap.GeekBurguer.Domain.Model.FoodRestrictions>(foodRestrictions);
                _repo.Inserir(restrictions);
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

        private static async Task<Face> DetectFaceAsync(Stream imageStream)
        {
            try
            {
                var faces = await faceServiceClient.DetectAsync(imageStream);
                return faces.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Guid?> AddFaceAsync(Guid faceListId, Stream imageStream)
        {
            try
            {
                AddPersistedFaceResult faceResult;
                faceResult = await faceServiceClient.AddFaceToFaceListAsync(faceListId.ToString(), imageStream);
                return faceResult.PersistedFaceId;
            }
            catch (Exception)
            {
                Console.WriteLine("Face not included in Face List!");
                return null;
            }
        }
        #endregion
    }
}