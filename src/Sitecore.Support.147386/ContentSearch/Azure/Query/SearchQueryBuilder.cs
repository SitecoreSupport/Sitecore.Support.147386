using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.ContentSearch.Azure.Query
{
    public class SearchQueryBuilder : Sitecore.ContentSearch.Azure.Query.SearchQueryBuilder
    {
        public override string Equal(string field, object expression, float boost)
        {
            if (!(expression is string))
            {
                throw new NotSupportedException("Only string expressions are supported by search");
            }

            if (string.IsNullOrEmpty(field) || field == "*")
            {
                return string.Format("&search={0}{1}", expression, boost != 1f ? "^" + boost : string.Empty);
            }
            //Sitecore.Support.147386 the additional quotes which surround our expression are added.
            return string.Format("&search={0}:({1}){2}", field, Escape((string)expression), boost != 1f ? "^" + boost : string.Empty);
        }
    }
}