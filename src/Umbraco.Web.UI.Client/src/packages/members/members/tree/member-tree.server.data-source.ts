import { UmbMemberTreeItemModel } from './types.js';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbTreeServerDataSourceBase } from '@umbraco-cms/backoffice/tree';

/**
 * A data source for the Member tree that fetches data from the server
 * @export
 * @class UmbMemberTreeServerDataSource
 * @extends {UmbTreeServerDataSourceBase}
 */
export class UmbMemberTreeServerDataSource extends UmbTreeServerDataSourceBase<
	EntityTreeItemResponseModel,
	UmbMemberTreeItemModel
> {
	/**
	 * Creates an instance of UmbMemberTreeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTreeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getChildrenOf,
			mapper,
		});
	}
}

const getChildrenOf = (parentUnique: string | null): any => {
	alert('not implemented');
};

const mapper = (item: EntityTreeItemResponseModel): UmbMemberTreeItemModel => {
	return {
		id: item.id!,
		parentId: item.parentId!,
		name: item.name!,
		type: 'member',
		hasChildren: item.hasChildren!,
		isContainer: item.isContainer!,
	};
};
