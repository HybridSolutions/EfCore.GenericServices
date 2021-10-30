// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using GenericServices;
using GenericServices.PublicButHidden;
using GenericServices.Setup;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Benchmarking
{
    public class PerfCallMethodUpdate2
    {
        private int _incdDay = 0;
        private DbContextOptions<EfCoreContext> _options;
        private SpecificUseData _utData;

        [Fact]
        public void ChangePublicationDate()
        {
            var summary = BenchmarkRunner.Run<PerfCallMethodUpdate2>();
        }

        [GlobalSetup]
        public void Setup()
        {
            _options = SqliteHelper.GetSqliteInMemoryOptions();
            using (var context = new EfCoreContext(_options))
            {
                context.Database.EnsureCreated();
                if (!context.Books.Any())
                    context.SeedDatabaseFourBooks();
                _utData = context.SetupSingleDtoAndEntities<Tests.Dtos.ChangePubDateDto>();
            }

        }

        [Benchmark]
        public void RunHandCodedPropetyUpdate()
        {
            //SETUP
            using (var context = new EfCoreContext(_options))
            {
                //ATTEMPT
                var newDate = new DateTime(2000, 1, 1).AddDays(_incdDay++);
                var book = context.Books.Single(x => x.BookId == 4);
                book.PublishedOn = newDate;
                context.SaveChanges();

                //VERIFY
                var entity = context.Books.Find(4);
                entity.PublishedOn.ShouldEqual(newDate);
            }
        }

        [Benchmark]
        public void RunGenericServicePropertyUpdate()
        {
            //SETUP
            using (var context = new EfCoreContext(_options))
            {
                var service = new CrudServices<EfCoreContext>(context, _utData.ConfigAndMapper);

                //ATTEMPT
                var newDate = new DateTime(2000, 1, 1).AddDays(_incdDay++);
                var dto = new Tests.Dtos.ChangePubDateDto { BookId = 4, PublishedOn = newDate };
                service.UpdateAndSave(dto, CrudValues.UseAutoMapper);

                //VERIFY
                service.IsValid.ShouldBeTrue(service.GetAllErrors());
                var entity = context.Books.Find(4);
                entity.PublishedOn.ShouldEqual(newDate);
            }
        }

        [Benchmark]
        public void RunHandCodedMethodAccess()
        {
            //SETUP
            using (var context = new EfCoreContext(_options))
            {
                //ATTEMPT
                var newDate = new DateTime(2000, 1, 1).AddDays(_incdDay++);
                var book = context.Books.Single(x => x.BookId == 4);
                book.UpdatePublishedOn(newDate);
                context.SaveChanges();

                //VERIFY
                var entity = context.Books.Find(4);
                entity.PublishedOn.ShouldEqual(newDate);
            }
        }

        [Benchmark]
        public void RunGenericServiceMethodAccess()
        {
            //SETUP
            using (var context = new EfCoreContext(_options))
            {
                var service = new CrudServices<EfCoreContext>(context, _utData.ConfigAndMapper);

                //ATTEMPT
                var newDate = new DateTime(2000, 1, 1).AddDays(_incdDay++);
                var dto = new Tests.Dtos.ChangePubDateDto { BookId = 4, PublishedOn = newDate };
                service.UpdateAndSave(dto, nameof(Book.UpdatePublishedOn));

                //VERIFY
                service.IsValid.ShouldBeTrue(service.GetAllErrors());
                var entity = context.Books.Find(4);
                entity.PublishedOn.ShouldEqual(newDate);
            }
        }
    }
}
