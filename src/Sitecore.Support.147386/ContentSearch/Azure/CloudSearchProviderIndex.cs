namespace Sitecore.Support.ContentSearch.Azure
{
    using System;
    using Sitecore.Diagnostics;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.Security;
    using System.Reflection;
  using Sitecore.ContentSearch.Azure.Schema;
  using Sitecore.ContentSearch.Azure.Http;

  public class CloudSearchProviderIndex : Sitecore.ContentSearch.Azure.CloudSearchProviderIndex
    {
        private static readonly MethodInfo EnsureInitializedMethodInfo =
          typeof(Sitecore.ContentSearch.Azure.CloudSearchProviderIndex).GetMethod("EnsureInitialized",
            BindingFlags.Instance | BindingFlags.NonPublic);
        public CloudSearchProviderIndex(string name, string connectionStringName, string totalParallelServices, IIndexPropertyStore propertyStore) : base(name, connectionStringName, totalParallelServices, propertyStore)
        {
        }

        public CloudSearchProviderIndex(string name, string connectionStringName, string totalParallelServices, IIndexPropertyStore propertyStore, string @group) : base(name, connectionStringName, totalParallelServices, propertyStore, @group)
        {
        }

    #region Workaround for issue 136614

    public new ICloudSearchIndexSchemaBuilder SchemaBuilder
    {
      get { return (this as Sitecore.ContentSearch.Azure.CloudSearchProviderIndex).SchemaBuilder; }
      set
      {
        var pi = typeof(Sitecore.ContentSearch.Azure.CloudSearchProviderIndex)
            .GetProperty("SchemaBuilder", BindingFlags.Instance | BindingFlags.Public);
        pi.SetValue(this, value);
      }
    }


    public new ISearchService SearchService
    {
      get { return (this as Sitecore.ContentSearch.Azure.CloudSearchProviderIndex).SearchService; }
      set
      {
        var pi = typeof(Sitecore.ContentSearch.Azure.CloudSearchProviderIndex)
            .GetProperty("SearchService", BindingFlags.Instance | BindingFlags.Public);
        pi.SetValue(this, value);
      }
    }

    #endregion
    public override void Initialize()
        {
            Log.Warn(string.Format("Sitecore Support: Initializing index {0}", this.Name), this);
            try
            {
                base.Initialize();
            }
            catch (Exception e)
            {
                Log.Error(string.Format("Sitecore Support: Initializing index {0} failed", e), this);
                throw;
            }
            finally
            {
                Log.Warn(string.Format("Sitecore Support: Initializing index {0} completed", this.Name), this);
            }
        }

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            EnsureInitializedMethodInfo.Invoke(this, new object[0]);
            return new Sitecore.Support.ContentSearch.Azure.CloudSearchSearchContext(this, options);
        }
    }
}