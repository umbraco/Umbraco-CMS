import type { UmbDataTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { DataTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Data Type items
 * @export
 * @class UmbDataTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbDataTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	DataTypeItemResponseModel,
	UmbDataTypeItemModel
> {
	/**
	 * Creates an instance of UmbDataTypeItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => DataTypeResource.getDataTypeItem({ id: uniques });

const mapper = (item: DataTypeItemResponseModel): UmbDataTypeItemModel => {
	return {
		unique: item.id,
		name: item.name,
		propertyEditorUiAlias: item.editorUiAlias || '', // TODO: why can this be undefined or null on the server?
	};
};
