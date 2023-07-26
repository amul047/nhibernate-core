﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NUnit.Framework;
using NHibernate.Linq;

namespace NHibernate.Test.Linq
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture(typeof(EnumType<TestEnum>), "0")]
	[TestFixture(typeof(EnumStringType<TestEnum>), "'Unspecified'")]
	[TestFixture(typeof(EnumAnsiStringType<TestEnum>), "'Unspecified'")]
	public class EnumTestsAsync : TestCaseMappingByCode
	{
		private IType _enumType;
		private string _unspecifiedValue;

		public EnumTestsAsync(System.Type enumType, string unspecifiedValue)
		{
			_enumType = (IType) Activator.CreateInstance(enumType);
			_unspecifiedValue = unspecifiedValue;
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<EnumEntity>(
				rc =>
				{
					rc.Table("EnumEntity");
					rc.Id(x => x.Id, m => m.Generator(Generators.Guid));
					rc.Property(x => x.Name);
					rc.Property(x => x.Enum1, m => m.Type(_enumType));
					rc.Property(x => x.NullableEnum1, m =>
					{
						m.Type(_enumType);
						m.Formula($"(case when Enum1 = {_unspecifiedValue} then null else Enum1 end)");
					});
					rc.Bag(x => x.Children, m => 
						{
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						a => a.OneToMany()
					);
					rc.ManyToOne(x => x.Other, m => m.Cascade(Mapping.ByCode.Cascade.All));
				});

			mapper.Class<EnumEntityChild>(
				rc =>
				{
					rc.Table("EnumEntityChild");
					rc.Id(x => x.Id, m => m.Generator(Generators.Guid));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				session.Save(new EnumEntity { Enum1 = TestEnum.Unspecified });
				session.Save(new EnumEntity { Enum1 = TestEnum.Small });
				session.Save(new EnumEntity { Enum1 = TestEnum.Small });
				session.Save(new EnumEntity { Enum1 = TestEnum.Medium });
				session.Save(new EnumEntity { Enum1 = TestEnum.Medium });
				session.Save(new EnumEntity { Enum1 = TestEnum.Medium });
				session.Save(new EnumEntity { Enum1 = TestEnum.Large });
				session.Save(new EnumEntity { Enum1 = TestEnum.Large });
				session.Save(new EnumEntity { Enum1 = TestEnum.Large });
				session.Save(new EnumEntity { Enum1 = TestEnum.Large });
				trans.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task CanQueryOnEnum_Large_4Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Large, 4));
		}

		[Test]
		public async Task CanQueryOnEnum_Medium_3Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Medium, 3));
		}

		[Test]
		public async Task CanQueryOnEnum_Small_2Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Small, 2));
		}

		[Test]
		public async Task CanQueryOnEnum_Unspecified_1Async()
		{
			await (CanQueryOnEnumAsync(TestEnum.Unspecified, 1));
		}

		private async Task CanQueryOnEnumAsync(TestEnum type, int expectedCount, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var query = await (session.Query<EnumEntity>().Where(x => x.Enum1 == type).ToListAsync(cancellationToken));

				Assert.AreEqual(expectedCount, query.Count);
			}
		}

		[Test]
		public async Task CanQueryWithContainsOnTestEnum_Small_1Async()
		{
			var values = new[] { TestEnum.Small, TestEnum.Medium };
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var query = await (session.Query<EnumEntity>().Where(x => values.Contains(x.Enum1)).ToListAsync());

				Assert.AreEqual(5, query.Count);
			}
		}

		[Test]
		public async Task ConditionalNavigationPropertyAsync()
		{
			TestEnum? type = null;
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var entities = session.Query<EnumEntity>();
				await (entities.Where(o => o.Enum1 == TestEnum.Large).ToListAsync());
				await (entities.Where(o => TestEnum.Large != o.Enum1).ToListAsync());
				await (entities.Where(o => (o.NullableEnum1 ?? TestEnum.Large) == TestEnum.Medium).ToListAsync());
				await (entities.Where(o => ((o.NullableEnum1 ?? type) ?? o.Enum1) == TestEnum.Medium).ToListAsync());

				await (entities.Where(o => (o.NullableEnum1.HasValue ? o.Enum1 : TestEnum.Unspecified) == TestEnum.Medium).ToListAsync());
				await (entities.Where(o => (o.Enum1 != TestEnum.Large
										? (o.NullableEnum1.HasValue ? o.Enum1 : TestEnum.Unspecified)
										: TestEnum.Small) == TestEnum.Medium).ToListAsync());

				await (entities.Where(o => (o.Enum1 == TestEnum.Large ? o.Other : o.Other).Name == "test").ToListAsync());
			}
		}

		[TestCase(null)]
		[TestCase(TestEnum.Unspecified)]
		public async Task CanQueryComplexExpressionOnTestEnumAsync(TestEnum? type)
		{
			using (var session = OpenSession())
			{
				var entities = session.Query<EnumEntity>();

				var query = await ((from user in entities
							 where (user.NullableEnum1 == TestEnum.Large
									   ? TestEnum.Medium
									   : user.NullableEnum1 ?? user.Enum1
								   ) == type
							 select new
							 {
								 user,
								 simple = user.Enum1,
								 condition = user.Enum1 == TestEnum.Large ? TestEnum.Medium : user.Enum1,
								 coalesce = user.NullableEnum1 ?? TestEnum.Medium
							 }).ToListAsync());

				Assert.That(query.Count, Is.EqualTo(type == TestEnum.Unspecified ? 1 : 0));
			}
		}

		[Test]
		public async Task CanProjectWithListTransformationAsync()
		{
			using (var session = OpenSession())
			using (var trans = session.BeginTransaction())
			{
				var entities = session.Query<EnumEntity>();

				var query = await (entities.Select(user => new
				{
					user.Name,
					simple = user.Enum1,
					children = user.Children,
					nullableEnum1IsLarge = user.NullableEnum1 == TestEnum.Large
				}).ToListAsync());

				Assert.That(query.Count, Is.EqualTo(10));
			}
		}
	}
}
