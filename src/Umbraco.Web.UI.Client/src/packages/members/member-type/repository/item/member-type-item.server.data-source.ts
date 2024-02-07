import type { UmbMemberTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { MemberTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Member Type items
 * @export
 * @class UmbMemberTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMemberTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	MemberTypeItemResponseModel,
	UmbMemberTypeItemModel
> {
	/**
	 * Creates an instance of UmbMemberTypeItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MemberTypeResource.getItemMemberType({ id: uniques });

const mapper = (item: MemberTypeItemResponseModel): UmbMemberTypeItemModel => {
	return {
		unique: item.id,
		name: item.name,
		propertyEditorUiAlias: item.editorUiAlias || '', // TODO: why can this be undefined or null on the server?
	};
};
