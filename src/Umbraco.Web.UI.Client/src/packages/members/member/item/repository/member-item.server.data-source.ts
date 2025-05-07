import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import type { UmbMemberItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Member items
 * @class UmbMemberItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbMemberItemServerDataSource extends UmbItemServerDataSourceBase<
	MemberItemResponseModel,
	UmbMemberItemModel
> {
	/**
	 * Creates an instance of UmbMemberItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
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
const getItems = (uniques: Array<string>) => MemberService.getItemMember({ query: { id: uniques } });

const mapper = (item: MemberItemResponseModel): UmbMemberItemModel => {
	return {
		entityType: UMB_MEMBER_ENTITY_TYPE,
		unique: item.id,
		name: item.variants[0].name || '',
		kind: item.kind,
		memberType: {
			unique: item.memberType.id,
			icon: item.memberType.icon,
			collection: item.memberType.collection ? { unique: item.memberType.collection.id } : null,
		},
		variants: item.variants.map((variant) => {
			return {
				name: variant.name,
				culture: variant.culture || null,
			};
		}),
	};
};
