using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Bogus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyCachingExample.Tests
{
    [TestClass]
    public class ICachedRepositoryTests
    {
        private ICachedRepository<Person> cachedRepository;
        private static int userId = 1;

        static ICachedRepositoryTests()
        {
            //// Set the randomizer seed to generate repeatable data sets.
            Randomizer.Seed = new Random(8675309);

            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((hostBuilder, configBuilder) =>
                    {
                        configBuilder.AddJsonFile("appsettings.json", optional: true);
                        configBuilder.AddEnvironmentVariables();
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .UseServiceProviderFactory(new CustomAutofacServiceProviderFactory())
                .Build();
        }

        private ICachedRepository<Person> Cache => cachedRepository ??= Context.Container.Resolve<ICachedRepository<Person>>();

        private static Faker<Person> FakePeople
        {
            get
            {
                return new Faker<Person>()
                    .RuleFor(x => x.Id, x => userId++)
                    .RuleFor(x => x.FamilyName, x => x.Name.LastName())
                    .RuleFor(x => x.GivenNames, x => x.Name.FirstName());
            }
        }

        [TestMethod]
        public void Get()
        {
            var person = FakePeople.Generate();
            Cache.Set($"Person_{person.Id}", person);
            var cachedPerson = Cache.Get($"Person_{person.Id}");
            Assert.IsNotNull(cachedPerson);
        }

        [TestMethod]
        public async Task GetAsync()
        {
            var person = FakePeople.Generate();
            await Cache.SetAsync($"Person_{person.Id}", person);
            var cachedPerson = await Cache.GetAsync($"Person_{person.Id}");
            Assert.IsNotNull(cachedPerson);
        }

        [TestMethod]
        public void GetOrAdd()
        {
            var person = FakePeople.Generate();
            var cachedPerson = Cache.GetOrSet($"Person_{person.Id}", () => person);
            Assert.IsNotNull(cachedPerson);
        }

        [TestMethod]
        public async Task GetOrAddAsync()
        {
            var person = FakePeople.Generate();
            var cachedPerson = await Cache.GetOrSetAsync($"Person_{person.Id}", async () => await Task.FromResult(person));
            Assert.IsNotNull(cachedPerson);
        }

        [TestMethod]
        public void Put()
        {
            var person = FakePeople.Generate();
            Cache.Set($"Person_{person.Id}", person);
            var cachedPerson = Cache.Get($"Person_{person.Id}");
            Assert.AreEqual(person, cachedPerson);
        }

        [TestMethod]
        public async Task PutAsync()
        {
            var person = FakePeople.Generate();
            await Cache.SetAsync($"Person_{person.Id}", person);
            var cachedPerson = await Cache.GetAsync($"Person_{person.Id}");
            Assert.AreEqual(person, cachedPerson);
        }

        [TestMethod]
        public void PutAll()
        {
            var people = FakePeople.GenerateBetween(10, 20);
            Cache.Set(people.ToDictionary(k => $"Person_{k.Id}", v => v));
            var cachedPeople = new List<Person>(people.Count);

            foreach (var person in people)
            {
                var cachedPerson = Cache.Get($"Person_{person.Id}");
                cachedPeople.Add(cachedPerson);
            }

            CollectionAssert.AreEqual(cachedPeople, people);
        }

        [TestMethod]
        public async Task PutAllAsync()
        {
            var people = FakePeople.GenerateBetween(10, 20);
            await Cache.SetAsync(people.ToDictionary(k => $"Person_{k.Id}", v => v));
            var cachedPeople = new List<Person>(people.Count);

            foreach (var person in people)
            {
                var cachedPerson = await Cache.GetAsync($"Person_{person.Id}");
                cachedPeople.Add(cachedPerson);
            }

            CollectionAssert.AreEqual(cachedPeople, people);
        }

        [TestMethod]
        public async Task RemoveAllAsync()
        {
            await Cache.RemoveAllAsync();
            // TODO: How to test? Manually check Redis db by GUI?
        }

        [TestMethod]
        public void Remove()
        {
            var person = FakePeople.Generate();
            Cache.Set($"Person_{person.Id}", person);
            Cache.Remove($"Person_{person.Id}");
            var cachedPerson = Cache.Get($"Person_{person.Id}");
            Assert.IsNull(cachedPerson);
        }

        [TestMethod]
        public async Task RemoveAsync()
        {
            var person = FakePeople.Generate();
            await Cache.SetAsync($"Person_{person.Id}", person);
            await Cache.RemoveAsync($"Person_{person.Id}");
            var cachedPerson = await Cache.GetAsync($"Person_{person.Id}");
            Assert.IsNull(cachedPerson);
        }

        [TestMethod]
        public async Task RemoveByPrefixAsync()
        {
            await Cache.RemoveByPrefixAsync("Person_");
            var cachedPerson = Cache.Get("Person_1");
            Assert.IsNull(cachedPerson);
        }

        [ClassCleanup]
        public static async Task Cleanup()
        {
            var cache = Context.Container.Resolve<ICachedRepository<Person>>();
            await cache.RemoveAllAsync();
        }
    }

    [Serializable]
    public record Person
    {
        public int Id { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }
    }
}