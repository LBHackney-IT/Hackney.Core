using System.Collections.Generic;
using Nest;

namespace Hackney.Core.ElasticSearch.Interfaces
{
    public interface IQueryBuilder<T> where T : class
    {
        public IQueryBuilder<T> WithWildstarQuery(string searchText, List<string> fields, TextQueryType textQueryType = TextQueryType.MostFields);

        public IQueryBuilder<T> WithFilterQuery(string commaSeparatedFilters, List<string> fields, TextQueryType textQueryType = TextQueryType.MostFields);

        public IQueryBuilder<T> WithExactQuery(string searchText, List<string> fields, IExactSearchQuerystringProcessor processor = null, TextQueryType textQueryType = TextQueryType.MostFields);

        public QueryContainer BuildSimpleQuery(QueryContainerDescriptor<T> containerDescriptor, string searchTerm, List<string> fields);
        public QueryContainer Build(QueryContainerDescriptor<T> containerDescriptor);
    }
}
