namespace Sitecore.Support.ContentSearch.Azure
{
    using System.Linq;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Azure.Query;
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.ContentSearch.Linq.Common;
    public class CloudSearchSearchContext : Sitecore.ContentSearch.Azure.CloudSearchSearchContext, IProviderSearchContext
    {
        public CloudSearchSearchContext(Sitecore.ContentSearch.Azure.CloudSearchProviderIndex index, SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck) : base(index, options)
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
            LinqToCloudIndex<TItem> linqToCloudIndex = new Sitecore.Support.ContentSearch.Azure.Query.LinqToCloudIndex<TItem>(this, executionContexts);
            if (this.Index.Locator.GetInstance<IContentSearchConfigurationSettings>().EnableSearchDebug())
            {
                ((IHasTraceWriter)linqToCloudIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }
            return linqToCloudIndex.GetQueryable();
        }
    }
}