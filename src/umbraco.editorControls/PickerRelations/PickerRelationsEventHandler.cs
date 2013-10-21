using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using umbraco.BusinessLogic; // ApplicationBase
using umbraco.businesslogic;
using umbraco.cms.businesslogic; // SaveEventArgs
using umbraco.cms.businesslogic.media; // Media
using umbraco.cms.businesslogic.member; // Member
using umbraco.cms.businesslogic.web; // Documentusing umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.relation;
using umbraco.DataLayer;

namespace umbraco.editorControls.PickerRelations
{
	/// <summary>
	/// Event handler that will convert a CSV into Relations
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PickerRelationsEventHandler : ApplicationStartupHandler
	{
        private enum PickerStorageFormat
        {
            Csv,
            Xml
        }

		/// <summary>
		/// Initializes a new instance of PickerRelationsEventHandler,
		/// hooks into the after event of saving a Content node, Media item or a Member
		/// </summary>
		public PickerRelationsEventHandler()
		{
			Document.AfterSave += new Document.SaveEventHandler(this.AfterSave);
			Media.AfterSave += new Media.SaveEventHandler(this.AfterSave);
			Member.AfterSave += new Member.SaveEventHandler(this.AfterSave);

			Document.BeforeDelete += new Document.DeleteEventHandler(this.BeforeDelete);
			Media.BeforeDelete += new Media.DeleteEventHandler(this.BeforeDelete);
			Member.BeforeDelete += new Member.DeleteEventHandler(this.BeforeDelete);
		}


		/// <summary>
		/// Event after all properties have been saved
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AfterSave(Content sender, SaveEventArgs e)
		{
			Guid pickerRelationsId  = new Guid(DataTypeGuids.PickerRelationsId);

			// For each PickerRelations datatype
			foreach (Property pickerRelationsProperty in from property in sender.GenericProperties
															  where property.PropertyType.DataTypeDefinition.DataType.Id == pickerRelationsId
															  select property)
			{
				// used to identify this datatype instance - relations created are marked with this in the comment field
				string instanceIdentifier = "[\"PropertyTypeId\":" + pickerRelationsProperty.PropertyType.Id.ToString() + "]";

				// get configuration options for datatype
				PickerRelationsOptions options = ((PickerRelationsPreValueEditor)pickerRelationsProperty.PropertyType.DataTypeDefinition.DataType.PrevalueEditor).Options;

				// find Picker source propertyAlias field on sender
				Property pickerProperty = sender.getProperty(options.PropertyAlias);

				if (pickerProperty != null)
				{
					// get relationType from options
					RelationType relationType = RelationType.GetById(options.RelationTypeId);

					if (relationType != null)
					{
						// validate: 1) check current type of sender matches that expected by the relationType, validation method is in the DataEditor
						uQuery.UmbracoObjectType contextObjectType = uQuery.UmbracoObjectType.Unknown;
						switch (sender.GetType().ToString())
						{
							case "umbraco.cms.businesslogic.web.Document": contextObjectType = uQuery.UmbracoObjectType.Document; break;
							case "umbraco.cms.businesslogic.media.Media": contextObjectType = uQuery.UmbracoObjectType.Media; break;
							case "umbraco.cms.businesslogic.member.Member": contextObjectType = uQuery.UmbracoObjectType.Member; break;
						}

						if (((PickerRelationsDataEditor)pickerRelationsProperty.PropertyType.DataTypeDefinition.DataType.DataEditor)
							.IsContextUmbracoObjectTypeValid(contextObjectType, relationType))
						{

							uQuery.UmbracoObjectType pickerUmbracoObjectType = uQuery.UmbracoObjectType.Unknown;

							// Get the object type expected by the associated relation type and if this datatype has been configures as a rever index
							pickerUmbracoObjectType = ((PickerRelationsDataEditor)pickerRelationsProperty.PropertyType.DataTypeDefinition.DataType.DataEditor)
														.GetPickerUmbracoObjectType(relationType);


							// clear all exisitng relations (or look to see previous verion of sender to delete changes ?)
							DeleteRelations(relationType, sender.Id, options.ReverseIndexing, instanceIdentifier);

							string pickerPropertyValue = pickerProperty.Value.ToString();

							var pickerStorageFormat = PickerStorageFormat.Csv; // Assume default of csv

                            if (xmlHelper.CouldItBeXml(pickerPropertyValue))
							{
                                pickerStorageFormat = PickerStorageFormat.Xml;
							}

							// Creating instances of Documents / Media / Members ensures the IDs are of a valid type - be quicker to check with GetUmbracoObjectType(int)
							Dictionary<int, string> pickerItems = null;
							switch (pickerUmbracoObjectType)
							{
								case uQuery.UmbracoObjectType.Document:
									switch (pickerStorageFormat)
									{
                                        case PickerStorageFormat.Csv:
											pickerItems = uQuery.GetDocumentsByCsv(pickerPropertyValue).ToNameIds();
											break;
                                        case PickerStorageFormat.Xml:
											pickerItems = uQuery.GetDocumentsByXml(pickerPropertyValue).ToNameIds();
											break;
									}

									break;
								case uQuery.UmbracoObjectType.Media:
									switch (pickerStorageFormat)
									{
                                        case PickerStorageFormat.Csv:
											pickerItems = uQuery.GetMediaByCsv(pickerPropertyValue).ToNameIds();
											break;
                                        case PickerStorageFormat.Xml:
											pickerItems = uQuery.GetMediaByXml(pickerPropertyValue).ToNameIds();
											break;
									}
									break;
								case uQuery.UmbracoObjectType.Member:
									switch (pickerStorageFormat)
									{
                                        case PickerStorageFormat.Csv:
											pickerItems = uQuery.GetMembersByCsv(pickerPropertyValue).ToNameIds();
											break;
                                        case PickerStorageFormat.Xml:
											pickerItems = uQuery.GetMembersByXml(pickerPropertyValue).ToNameIds();
											break;
									}
									break;
							}
							if (pickerItems != null)
							{
								foreach (KeyValuePair<int, string> pickerItem in pickerItems)
								{
									CreateRelation(relationType, sender.Id, pickerItem.Key, options.ReverseIndexing, instanceIdentifier);
								}
							}
						}
						else
						{
							// Error: content object type invalid with relation type
						}
					}
					else
					{
						// Error: relation type is null
					}
				}
				else
				{
					// Error: pickerProperty alias not found
				}
			}
		}

		/// <summary>
		/// Clears any existing relations when deleting a node with a PickerRelations datatype
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="umbraco.cms.businesslogic.DeleteEventArgs"/> instance containing the event data.</param>
		private void BeforeDelete(Content sender, DeleteEventArgs e)
		{
			Guid pickerRelationsId = new Guid(DataTypeGuids.PickerRelationsId);

			// Clean up any relations

			// For each PickerRelations datatype
			foreach (Property pickerRelationsProperty in from property in sender.GenericProperties
															  where property.PropertyType.DataTypeDefinition.DataType.Id == pickerRelationsId
															  select property)
			{
				// used to identify this datatype instance - relations created are marked with this in the comment field
				string instanceIdentifier = "[\"PropertyTypeId\":" + pickerRelationsProperty.PropertyType.Id.ToString() + "]";

				// get configuration options for datatype
				PickerRelationsOptions options = ((PickerRelationsPreValueEditor)pickerRelationsProperty.PropertyType.DataTypeDefinition.DataType.PrevalueEditor).Options;

				// get relationType from options
				RelationType relationType = RelationType.GetById(options.RelationTypeId);

				if (relationType != null)
				{
					// clear all exisitng relations
					DeleteRelations(relationType, sender.Id, options.ReverseIndexing, instanceIdentifier);
				}
			}
		}

		/// <summary>
		/// Delete all relations using the content node for a given RelationType
		/// </summary>
		/// <param name="relationType"></param>
		/// <param name="contentNodeId"></param>
		/// <param name="reverseIndexing"></param>
		/// <param name="instanceIdentifier">NOT USED ATM</param>
		private static void DeleteRelations(RelationType relationType, int contentNodeId, bool reverseIndexing, string instanceIdentifier)
		{
			//if relationType is bi-directional or a reverse index then we can't get at the relations via the API, so using SQL
			string getRelationsSql = "SELECT id FROM umbracoRelation WHERE relType = " + relationType.Id.ToString() + " AND ";

			if (reverseIndexing || relationType.Dual)
			{
				getRelationsSql += "childId = " + contentNodeId.ToString();
			}
			if (relationType.Dual) // need to return relations where content node id is used on both sides
			{
				getRelationsSql += " OR ";
			}
			if (!reverseIndexing || relationType.Dual)
			{
				getRelationsSql += "parentId = " + contentNodeId.ToString();
			}

			getRelationsSql += " AND comment = '" + instanceIdentifier + "'";

			using (IRecordsReader relations = uQuery.SqlHelper.ExecuteReader(getRelationsSql))
			{
				//clear data
				Relation relation;
				while (relations.Read())
				{
					relation = new Relation(relations.GetInt("id"));

					// TODO: [HR] check to see if an instance identifier is used
					relation.Delete();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="relationType"></param>
		/// <param name="contentNodeId">id sourced from the Content / Media / Member</param>
		/// <param name="pickerNodeId">id sourced from the Picker</param>
		/// <param name="reverseIndexing">if true, reverses the parentId and child Id</param>
		/// <param name="instanceIdentifier">JSON string with id of Picker Relations property instance</param>
		private static void CreateRelation(RelationType relationType, int contentNodeId, int pickerNodeId, bool reverseIndexing, string instanceIdentifier)
		{
			if (reverseIndexing)
			{
				Relation.MakeNew(pickerNodeId, contentNodeId, relationType, instanceIdentifier);
			}
			else
			{
				Relation.MakeNew(contentNodeId, pickerNodeId, relationType, instanceIdentifier);
			}
		}
	}
}
