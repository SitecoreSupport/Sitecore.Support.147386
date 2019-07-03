namespace Sitecore.Support.ContentSearch.Azure.Query
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Azure.Query;
    using Sitecore.ContentSearch.Azure;

    public class LinqToCloudIndex<TItem> : Sitecore.ContentSearch.Azure.Query.LinqToCloudIndex<TItem>
    {
        private static ConcurrentDictionary<Type, FieldInfo> queryMapperFieldInfos = new ConcurrentDictionary<Type, FieldInfo>();
        public LinqToCloudIndex(Sitecore.ContentSearch.Azure.CloudSearchSearchContext context, IExecutionContext executionContext) : base(context, executionContext)
        {
        }

        public LinqToCloudIndex(Sitecore.ContentSearch.Azure.CloudSearchSearchContext context, IExecutionContext[] executionContexts, ServiceCollectionClient serviceCollectionClient) : base(context, executionContexts, serviceCollectionClient)
        {
            FieldInfo queryMapperFieldInfo;
            var type = this.GetType().BaseType.BaseType;
            if (queryMapperFieldInfos.TryGetValue(type, out queryMapperFieldInfo))
            {
                queryMapperFieldInfo.SetValue(this, new Sitecore.Support.ContentSearch.Azure.Query.CloudQueryMapper(this.Parameters));
            }
            else
            {
                queryMapperFieldInfo = type.GetField("queryMapper", BindingFlags.Instance | BindingFlags.NonPublic);
                queryMapperFieldInfos.TryAdd(type, queryMapperFieldInfo);
                queryMapperFieldInfo.SetValue(this, new Sitecore.Support.ContentSearch.Azure.Query.CloudQueryMapper(this.Parameters));
            }
        }
        private TResult ApplyScalarMethods<TResult, TDocument>(CloudQuery query,
          CloudSearchResults<TDocument> processedResults, int? totalCount)
        {
            Type documentType = typeof(TResult).GetGenericArguments()[0];
            var applyScalarMethodsMethod = this.GetType().BaseType.GetMethod("ApplyScalarMethods", BindingFlags.Instance | BindingFlags.NonPublic);
            var applyScalarMethodsGenericMethod =
              applyScalarMethodsMethod.MakeGenericMethod(new Type[] { typeof(TResult), documentType });
            return (TResult)applyScalarMethodsGenericMethod.Invoke(this, new object[] { query, processedResults, totalCount });
        }
    }
}