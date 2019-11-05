﻿using System;
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
        #endregion

        public UsersController()
        {
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
                    Restrictions = new List<string>() { "soja", "gluten" },
                    Others = "brocolis"
                }
            );
            listaRestricoes.Add(
                new FoodRestrictions
                {
                    RequesterId = Guid.NewGuid(),
                    UserId = UserIdDois,
                    Restrictions = new List<string>() { "lactose" },
                    Others = "ovos"
                }
            );
        }


        [HttpGet]
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
                        {
                            persistedId = AddFaceAsync(FaceListId, imageStream).Result;
                            Console.WriteLine($"New User with FaceId {persistedId}");
                        }
                        else
                            Console.WriteLine($"Face Exists with Face {persistedId}");
                    }
                    else
                    {
                        Console.WriteLine("Not a face!");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Probably Rate Limit for API was reached, please try again later");
                }
            }

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