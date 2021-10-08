﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hackney.Core.Elastic.Interfaces;
using Nest;

namespace Hackney.Core.Elastic
{
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class
    {
        private readonly IWildCardAppenderAndPrepender _wildCardAppenderAndPrepender;
        private readonly List<Func<QueryContainerDescriptor<T>, QueryContainer>> _queries;
        private string _searchQuery;
        private string _filterQuery;

        public QueryBuilder(IWildCardAppenderAndPrepender wildCardAppenderAndPrepender)
        {
            _wildCardAppenderAndPrepender = wildCardAppenderAndPrepender;
            _queries = new List<Func<QueryContainerDescriptor<T>, QueryContainer>>();
        }

        public IQueryBuilder<T> CreateWildstarSearchQuery(string searchText)
        {
            var listOfWildCardedWords = _wildCardAppenderAndPrepender.Process(searchText);
            _searchQuery = $"({string.Join(" AND ", listOfWildCardedWords)}) " +
                           string.Join(' ', listOfWildCardedWords);

            return this;
        }

        public IQueryBuilder<T> CreateFilterQuery(string commaSeparatedFilters)
        {
            _filterQuery = string.Join(' ', commaSeparatedFilters.Split(","));

            return this;
        }

        public IQueryBuilder<T> SpecifyFieldsToBeSearched(List<string> fields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(_searchQuery)
                        .Type(TextQueryType.MostFields)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            _queries.Add(query);

            return this;
        }

        public IQueryBuilder<T> SpecifyFieldsToBeFiltered(List<string> fields)
        {
            Func<QueryContainerDescriptor<T>, QueryContainer> query =
                (containerDescriptor) => containerDescriptor.QueryString(q =>
                {
                    var queryDescriptor = q.Query(_filterQuery)
                        .Type(TextQueryType.MostFields)
                        .Fields(f =>
                        {
                            foreach (var field in fields)
                            {
                                f = f.Field(field);
                            }

                            return f;
                        });

                    return queryDescriptor;
                });

            _queries.Add(query);

            return this;
        }

        public QueryContainer FilterAndRespectSearchScore(QueryContainerDescriptor<T> containerDescriptor)
        {
            return containerDescriptor.Bool(builder => builder.Must(_queries));
        }

        public QueryContainer Search(QueryContainerDescriptor<T> containerDescriptor)
        {
            return _queries.First().Invoke(containerDescriptor);
        }
    }
}