import type { UmbMemberItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { MemberResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Member items
 * @export
 * @class UmbMemberItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMemberItemServerDataSource extends UmbItemServerDataSourceBase<
	MemberItemResponseModel,
	UmbMemberItemModel
> {
	/**
	 * Creates an instance of UmbMemberItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MemberResource.getItemMember({ id: uniques });

const mapper = (item: MemberItemResponseModel): UmbMemberItemModel => {
	return {
		unique: item.id,
		memberType: {
			unique: item.memberType.id,
			icon: item.memberType.icon,
			hasListView: item.memberType.hasListView,
		},
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
			};
		}),
	};
};
