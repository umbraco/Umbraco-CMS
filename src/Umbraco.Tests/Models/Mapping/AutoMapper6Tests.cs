using System;
using AutoMapper;
using NUnit.Framework;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    public class AutoMapper6Tests
    {
        [Test]
        public void Test1()
        {
            ThingProfile.CtorCount = 0;
            Assert.AreEqual(0, ThingProfile.CtorCount);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ThingProfile1>();
            });

            Assert.AreEqual(1, ThingProfile.CtorCount);
            Assert.AreEqual(0, MemberValueResolver.CtorCount);

            var mapper = config.CreateMapper();

            Assert.AreEqual(1, ThingProfile.CtorCount);
            Assert.AreEqual(0, MemberValueResolver.CtorCount);

            var thingA = new ThingA { ValueInt = 42, ValueString = "foo" };
            var thingB = mapper.Map<ThingA, ThingB>(thingA);
            Assert.AreEqual(42, thingB.ValueInt);
            Assert.AreEqual("!!foo!!", thingB.ValueString);

            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
            Assert.AreEqual(1, ThingProfile.CtorCount); // one single profile
            Assert.AreEqual(4, MemberValueResolver.CtorCount); // many resolvers
        }

        [Test]
        public void Test2()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ThingProfile2>();
            });

            var mapper = config.CreateMapper();

            Assert.AreEqual(0, ValueResolver.CtorCount);

            var thingA = new ThingA { ValueInt = 42, ValueString = "foo" };
            var thingB = mapper.Map<ThingA, ThingB>(thingA);
            Assert.AreEqual(42, thingB.ValueInt);
            Assert.AreEqual("!!foo!!", thingB.ValueString);

            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
            Assert.AreEqual(4, ValueResolver.CtorCount); // many resolvers
        }

        [Test]
        public void Test3()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ThingProfile3>();
            });

            var mapper = config.CreateMapper();

            var thingA = new ThingA { ValueInt = 42, ValueString = "foo" };
            var thingB = mapper.Map<ThingA, ThingB>(thingA);
            Assert.AreEqual(42, thingB.ValueInt);
            Assert.AreEqual("!!foo!!", thingB.ValueString);

            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
            mapper.Map<ThingA, ThingB>(thingA);
        }

        // Resolve destination member using a custom value resolver
        // void ResolveUsing<TValueResolver>()
        //   where TValueResolver : IValueResolver<TSource, TDestination, TDestMember>;

        // Resolve destination member using a custom value resolver from a source member
        // void ResolveUsing<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        //   where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;
        // void ResolveUsing<TValueResolver, TSourceMember>(string sourceMemberName)
        //   where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;

        // Resolve destination member using a custom value resolver instance
        // void ResolveUsing(IValueResolver<TSource, TDestination, TMember> valueResolver);
        // void ResolveUsing<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember);

        // Resolve destination member using a custom value resolver callback
        // void ResolveUsing<TResult>(Func<TSource, TResult> resolver);
        // void ResolveUsing<TResult>(Func<TSource, TDestination, TResult> resolver);
        // void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, TResult> resolver);
        // void ResolveUsing<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> resolver);

        // read https://stackoverflow.com/questions/14875075/automapper-what-is-the-difference-between-mapfrom-and-resolveusing
        // about the diff between MapFrom and ResolveUsing... keeping ResolveUsing in our code

        public class ThingProfile : Profile
        {
            public static int CtorCount { get; set; }

            public ThingProfile(int ver)
            {
                CtorCount++;

                var map = CreateMap<ThingA, ThingB>()
                    .ForMember(dest => dest.ValueInt, opt => opt.MapFrom(src => src.ValueInt));

                switch (ver)
                {
                    case 0:
                        break;
                    case 1:
                        map
                            .ForMember(dest => dest.ValueString, opt => opt.MapFrom<MemberValueResolver, string>(src => src.ValueString));
                        break;
                    case 2:
                        map
                            .ForMember(dest => dest.ValueString, opt => opt.MapFrom<ValueResolver>());
                        break;
                    case 3:
                        // in most cases that should be perfectly enough?
                        map
                            .ForMember(dest => dest.ValueString, opt => opt.MapFrom(source => "!!" + source.ValueString + "!!"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ver));
                }
            }
        }

        public class ThingProfile1 : ThingProfile
        {
            public ThingProfile1() : base(1) { }
        }

        public class ThingProfile2 : ThingProfile
        {
            public ThingProfile2() : base(2) { }
        }

        public class ThingProfile3 : ThingProfile
        {
            public ThingProfile3() : base(3) { }
        }

        public class ValueResolver : IValueResolver<ThingA, ThingB, string>
        {
            public static int CtorCount { get; set; }

            public ValueResolver()
            {
                CtorCount++;
            }

            public string Resolve(ThingA source, ThingB destination, string destMember, ResolutionContext context)
            {
                return "!!" + source.ValueString + "!!";
            }
        }

        public class MemberValueResolver : IMemberValueResolver<ThingA, ThingB, string, string>
        {
            public static int CtorCount { get; set; }

            public MemberValueResolver()
            {
                CtorCount++;
            }

            public string Resolve(ThingA source, ThingB destination, string sourceMember, string destMember, ResolutionContext context)
            {
                return "!!" + sourceMember + "!!";
            }
        }

        public class ThingA
        {
            public int ValueInt { get; set; }
            public string ValueString { get; set; }
        }

        public class ThingB
        {
            public int ValueInt { get; set; }
            public string ValueString { get; set; }
        }
    }
}
