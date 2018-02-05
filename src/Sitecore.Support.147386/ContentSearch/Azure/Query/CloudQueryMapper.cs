using Sitecore.ContentSearch.Azure.Http;
using Sitecore.ContentSearch.Azure.Query;
using Sitecore.ContentSearch.Azure.Schema;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Helpers;
using Sitecore.ContentSearch.Linq.Nodes;
using System;
using System.Reflection;

namespace Sitecore.Support.ContentSearch.Azure.Query
{
    public class CloudQueryMapper : Sitecore.ContentSearch.Azure.Query.CloudQueryMapper
    {
        private readonly FieldInfo queryBuilder = typeof(Sitecore.ContentSearch.Azure.Query.CloudQueryMapper).GetField("queryBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo useIsMatchScoring = typeof(Sitecore.ContentSearch.Azure.Query.CloudQueryMapper).GetField("useIsMatchScoring", BindingFlags.NonPublic | BindingFlags.Instance);
        public CloudQueryMapper(CloudIndexParameters parameters) : base(parameters)
        {
        }
        private ICloudSearchIndexSchema Schema
        {
            get
            {
                return this.Parameters.Schema as ICloudSearchIndexSchema;
            }
        }

        protected override string HandleEqual(EqualNode node, CloudQueryMapperState state)
        {
            var onlyConstants = node.LeftNode is ConstantNode && node.RightNode is ConstantNode;

            if (onlyConstants)
            {
                var comparison = ((ConstantNode)node.LeftNode).Value.Equals(((ConstantNode)node.RightNode).Value);

                var expression = comparison
                                     ? SearchQueryBuilder.SearchForEverything
                                     : SearchQueryBuilder.SearchForNothing;

                return new Sitecore.Support.ContentSearch.Azure.Query.SearchQueryBuilder().Equal(null, expression, node.Boost);
            }

            var fieldNode = QueryHelper.GetFieldNode(node);
            var valueNode = QueryHelper.GetValueNode(node, fieldNode.FieldType);

            string query = null;
            if (this.ProcessAsVirtualField(fieldNode.FieldKey, valueNode.Value, node.Boost, ComparisonType.Equal, state, out query))
            {
                return query;
            }

            return this.HandleEqual(fieldNode.FieldKey, valueNode.Value, node.Boost);
        }

        private string HandleEqual(string initFieldName, object fieldValue, float boost)
        {
            var fieldName = this.Parameters.FieldNameTranslator.GetIndexFieldName(initFieldName, this.Parameters.IndexedFieldType);
            var fieldSchema = this.Schema.GetFieldByCloudName(fieldName);
            if (fieldSchema == null)
            {
                var expression = fieldValue == null ?
                    SearchQueryBuilder.SearchForEverything :
                    SearchQueryBuilder.SearchForNothing;

                return new Sitecore.Support.ContentSearch.Azure.Query.SearchQueryBuilder().Equal(null, expression, boost);
            }

            var formattedValue = this.ValueFormatter.FormatValueForIndexStorage(fieldValue, fieldName);

            if (fieldSchema.Type == EdmTypes.StringCollection)
            {
                return ((QueryStringBuilder)queryBuilder.GetValue(this)).FilterQueryBuilder.Any(fieldName, formattedValue, fieldSchema.Type);
            }

            if (formattedValue == null)
            {
                return $"&$filter={fieldName} eq null";
            }

            if (formattedValue is string)
            {
                if (boost > 1f && !(bool)useIsMatchScoring.GetValue(this))
                {
                    throw new NotSupportedException("Boost greater than 1 is supported only for ismatchscoring statements");
                }

                if (formattedValue.ToString().Trim() == string.Empty)
                {
                    return ((QueryStringBuilder)queryBuilder.GetValue(this)).FilterQueryBuilder.Equal(fieldName, formattedValue, fieldSchema.Type, boost);
                }

                return new Sitecore.Support.ContentSearch.Azure.Query.SearchQueryBuilder().Equal(fieldName, formattedValue, boost);
            }

            // Non string value
            if ((bool)useIsMatchScoring.GetValue(this))
            {
                //Boost on non-string values supported only with ismatchscoring 
                return ((QueryStringBuilder)queryBuilder.GetValue(this)).FilterQueryBuilder.Equal(fieldName, formattedValue, fieldSchema.Type, boost);
            }
            return ((QueryStringBuilder)queryBuilder.GetValue(this)).FilterQueryBuilder.Equal(fieldName, formattedValue, fieldSchema.Type, 1f);
        }
    }
}