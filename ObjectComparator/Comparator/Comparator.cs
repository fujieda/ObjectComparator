using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator
{
    public sealed class Comparator : ICompareObjectStrategy
    {
        public Comparator()
        {
            RuleForValuesTypes = new Rule<ICompareStructStrategy>(new CompareValueTypesStrategy());
            RuleForCollectionTypes = new RuleForCollections(this,
                new BaseCollectionsCompareStrategy[] {new DictionaryCompareStrategy()});
            RuleForReferenceTypes = new Rule<ICompareObjectStrategy>(this);
        }

        public Rule<ICompareObjectStrategy> RuleForReferenceTypes { get; }
        public Rule<ICompareStructStrategy> RuleForValuesTypes { get; }
        public RuleForCollections RuleForCollectionTypes { get; }

        public bool IsValid(Type member) => member.IsClass && member != typeof(string);

        public IList<string> Ignore { get; set; } = new List<string>();
        public IDictionary<string, ICompareValues> Strategies { get; set; } = new Dictionary<string, ICompareValues>();

        public Distinctions Compare<T>(T valueA, T valueB, string propertyName)
        {
            if (valueA != null && valueB == null || valueA == null && valueB != null)
                return Distinctions.Create(new Distinction(typeof(T).Name, valueA, valueB));

            var diff = new Distinctions();
            if (ReferenceEquals(valueA, valueB)) return diff;

            var type = valueA.GetType();

            foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
            {
                var name = mi.Name;
                var actualPropertyPath = MemberPathBuilder.BuildMemberPath(propertyName, mi);

                if (Ignore.Contains(actualPropertyPath)) continue;

                object firstValue = null;
                object secondValue = null;
                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        firstValue = type.GetField(name).GetValue(valueA);
                        secondValue = type.GetField(name).GetValue(valueB);
                        break;
                    case MemberTypes.Property:
                        firstValue = type.GetProperty(name).GetValue(valueA);
                        secondValue = type.GetProperty(name).GetValue(valueB);
                        break;
                }

                var diffRes = GetDistinctions(actualPropertyPath, firstValue, secondValue);
                if (diffRes.IsNotEmpty()) diff.AddRange(diffRes);
            }

            return diff;
        }

        private Distinctions GetDifference<T>(T valueA, T valueB, string propertyName) =>
            RuleFactory
                .Create(RuleForCollectionTypes, RuleForReferenceTypes, RuleForValuesTypes)
                .GetFor(valueB.GetType())
                .Compare(valueA, valueB, propertyName);

        public Distinctions Compare<T>(T objectA, T objectB) => Compare(objectA, objectB, null);

        public Distinctions GetDistinctions(string propertyName, dynamic valueA, dynamic valueB)
        {
            if (Strategies.IsNotEmpty() && Strategies.Any(x => x.Key == propertyName))
                return Strategies[propertyName].Compare(valueA, valueB, propertyName);

            var diff = new Distinctions();
            if (valueA == null && valueB != null) return Distinctions.Create(propertyName, "null", valueB);

            if (valueA != null && valueB == null) return Distinctions.Create(propertyName, valueA, "null");

            return valueA == null
                ? diff
                : (Distinctions) GetDifference(valueA, valueB, propertyName);
        }

        public void SetIgnore(IList<string> ignore)
        {
            if (ignore.IsEmpty()) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Ignore = ignore);
        }

        public void SetStrategies(IDictionary<string, ICompareValues> strategies)
        {
            if (strategies.IsEmpty()) return;
            RuleForReferenceTypes.Strategies.ForEach(x => x.Strategies = strategies);
        }
    }
}