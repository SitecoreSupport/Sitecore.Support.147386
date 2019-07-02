namespace Sitecore.Support.ContentSearch.Azure
{
    using System.Linq;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Azure.Query;
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Azure;
    using System.Reflection;

    public class CloudSearchSearchContext : Sitecore.ContentSearch.Azure.CloudSearchSearchContext, IProviderSearchContext
    {
        private readonly FieldInfo serviceCollectionClient = typeof(Sitecore.ContentSearch.Azure.CloudSearchSearchContext).GetField("serviceCollectionClient", BindingFlags.NonPublic | BindingFlags.Instance);

        public CloudSearchSearchContext(ServiceCollectionClient serviceCollectionClient, SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck) : base(serviceCollectionClient, options)
        {
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>()
        {
            return (this as IProviderSearchContext).GetQueryable<TItem>(new IExecutionContext[0]);
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return (this as IProviderSearchContext).GetQueryable<TItem>(new IExecutionContext[]
            {
        executionContext
            });
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            LinqToCloudIndex<TItem> linqToCloudIndex = new Sitecore.Support.ContentSearch.Azure.Query.LinqToCloudIndex<TItem>(this, executionContexts, (ServiceCollectionClient)serviceCollectionClient.GetValue(this));
            if (this.Index.Locator.GetInstance<IContentSearchConfigurationSettings>().EnableSearchDebug())
            {
                ((IHasTraceWriter)linqToCloudIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }
            return linqToCloudIndex.GetQueryable();
        }
    }
}