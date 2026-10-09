import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbCollectionConfiguration } from '@umbraco-cms/backoffice/collection';
import type { UmbDataTypeDetailModel } from '@umbraco-cms/backoffice/data-type';

/**
 * Maps the configuration of a collection data type onto the configuration of a collection.
 *
 * Only the parts the data type describes are returned. The entity the collection is scoped to is the host's to supply,
 * as a data type says nothing about what is being browsed.
 *
 * `dataTypeId` is deliberately never set. It reaches the server as `dataTypeKey`, which resolves a collection
 * configured as a *property* of a content type; supplying it for a collection configured on the content type itself
 * makes the request fail.
 * @param {UmbDataTypeDetailModel} dataType - The data type holding the collection configuration.
 * @returns {UmbCollectionConfiguration} The collection configuration described by the data type.
 */
export function umbMapDataTypeToCollectionConfiguration(dataType: UmbDataTypeDetailModel): UmbCollectionConfiguration {
	const config = new UmbPropertyEditorConfigCollection(dataType.values);
	const pageSize = Number(config.getValueByAlias('pageSize'));

	return {
		layouts: config.getValueByAlias('layouts'),
		orderBy: config.getValueByAlias('orderBy') ?? 'updateDate',
		orderDirection: config.getValueByAlias('orderDirection') ?? 'asc',
		pageSize: isNaN(pageSize) ? 50 : pageSize,
		userDefinedProperties: config.getValueByAlias('includeProperties'),
	};
}
