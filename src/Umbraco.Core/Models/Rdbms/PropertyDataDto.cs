using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsPropertyData")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class PropertyDataDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("contentNodeId")]
        public int NodeId { get; set; }

        [Column("versionId")]
        public Guid? VersionId { get; set; }

        [Column("propertytypeid")]
        public int PropertyTypeId { get; set; }

        [Column("dataInt")]
        public int? Integer { get; set; }

        [Column("dataDate")]
        public DateTime? Date { get; set; }

        [Column("dataNvarchar")]
        public string VarChar { get; set; }

        [Column("dataNtext")]
        public string Text { get; set; }

        [ResultColumn]
        public PropertyTypeDto PropertyTypeDto { get; set; }

        public object GetValue
        {
            get
            {
                if(Integer.HasValue)
                {
                    return Integer.Value;
                }
                
                if(Date.HasValue)
                {
                    return Date.Value;
                }
                
                if(!string.IsNullOrEmpty(VarChar))
                {
                    return VarChar;
                }

                if(!string.IsNullOrEmpty(Text))
                {
                    return Text;
                }

                return null;
            }
        }
    }
}