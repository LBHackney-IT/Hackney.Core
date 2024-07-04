using System;
using System.Collections.Generic;
using System.Linq;
using Hackney.Core.ElasticSearch.Interfaces;
using Nest;

namespace Hackney.Core.ElasticSearch
{
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class
    {
        private readonly IWildCardAppenderAndPrepender _wildCardAppenderAndPrepender;
        private Func<QueryContainerDescriptor<T>, QueryContainer> _wildstarQuery;
        private Func<QueryContainerDescriptor<T>, QueryContainer> _exactQuery;
        private List<Func<QueryContainerDescriptor<T>, QueryContainer>> _filterQueries;


        public QueryBuilder(IWildCardAppenderAndPrepender wildCardAppenderAndPrepender)
        {
            _wildCardAppenderAndPrepender = wildCardAppenderAndPrepender;
        }

        public IQueryBuilder<T> WithWildstarQuery(string searchText, List<string> fields, TextQueryType textQueryType = TextQueryType.MostFields)
        {
            var listOfWildCardedWords = _wildCardAppenderAndPrepender.Process(searchText);
            var queryString = $"({string.Join(" AND ", listOfWildCardedWords)}) " +
                              string.Join(' ', listOfWildCardedWords);

            _wildstarQuery = CreateQuery(queryString, fields, null, textQueryType);

            return this;
        }

        public IQueryBuilder<T> WithWildstarBoolQuery(string searchText, List<string> fields, int? minimumShouldMatch = 1, TextQueryType textQueryType = TextQueryType.MostFields)
        {
            var listOfWildCardedWords = _wildCardAppenderAndPrepender.Process(searchText);

            _wildstarQuery = CreateWildcardBoolQuery(listOfWildCardedWords, fields);

            return this;
        }


        public IQueryBuilder<T> WithFilterQuery(string commaSeparatedFilters, List<string> fields, TextQueryType textQueryType = TextQueryType.MostFields)
        {
            if (commaSeparatedFilters != null)
            {
                _filterQueries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
                foreach (var filterWord in commaSeparatedFilters.Split(","))
                {
                    _filterQueries.Add(CreateQuery(filterWord, fields, null, textQueryType));
                }
            }

            return this;
        }

        public IQueryBuilder<T> WithExactQuery(string searchText, List<string> fields,
            IExactSearchQuerystringProcessor processor = null, TextQueryType textQueryType = TextQueryType.MostFields)
        {
            if (processor != null)
                searchText = processor.Process(searchText);

            _exactQuery = CreateQuery(searchText, fields, 20, textQueryType);

            return this;
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> CreateQuery(string queryString,
            List<string> fields, double? boostValue = null, TextQueryType textQueryType = TextQueryType.MostFields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(queryString)
                        .Type(textQueryType)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field, boostValue);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            return query;
        }

        public QueryContainer Build(QueryContainerDescriptor<T> containerDescriptor)
        {
            var queryContainer = containerDescriptor.Bool(x => x.Should(_wildstarQuery, _exactQuery));

            if (_filterQueries != null)
            {
                var listOfFunctions = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
                listOfFunctions.AddRange(_filterQueries);

                queryContainer = containerDescriptor.Bool(x =>
                    x.Must(containerDescriptor.Bool(x => x.Should(listOfFunctions)),
                    queryContainer));
            }

            return queryContainer;
        }

        public QueryContainer BuildSimpleQuery(QueryContainerDescriptor<T> containerDescriptor, string searchTerm, List<string> fields)
        {
            return containerDescriptor.SimpleQueryString(q => q.Fields(f =>
                {
                    foreach (var field in fields)
                    {
                        f = f.Field(field);
                    }
                    return f;
                }
            ).Query(searchTerm));
        }

        private static Func<QueryContainerDescriptor<T>, QueryContainer> CreateWildcardBoolQuery(
                    List<string> words, List<string> fields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.Bool(b => b
                    .Should(fields.Select(field =>
                        (QueryContainer)new BoolQuery
                        {
                            Should = words.Select(word =>
                                (QueryContainer)new WildcardQuery
                                {
                                    Value = word,
                                    Field = field
                                }).ToList(),
                            MinimumShouldMatch = words.Count
                        }).ToArray()
                    )
                );

            return query;
        }
    }
}