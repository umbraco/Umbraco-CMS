import type { UmbUserGroupItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { UserGroupItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UserGroupResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for User Group items
 * @export
 * @class UmbUserGroupItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbUserGroupItemServerDataSource extends UmbItemServerDataSourceBase<
	UserGroupItemResponseModel,
	UmbUserGroupItemModel
> {
	/**
	 * Creates an instance of UmbUserGroupItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbUserGroupItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => UserGroupResource.getItemUserGroup({ id: uniques });

const mapper = (item: UserGroupItemResponseModel): UmbUserGroupItemModel => {
	return {
		unique: item.id,
		name: item.name,
		propertyEditorUiAlias: item.editorUiAlias || '', // TODO: why can this be undefined or null on the server?
	};
};
