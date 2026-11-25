import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbMemberTypeItemStore } from './member-type-item.store.js';

export const UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.MemberTypeItem';
export const UMB_MEMBER_TYPE_STORE_ALIAS = 'Umb.Store.MemberTypeItem';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.MemberType',
		name: 'Member Type Entity Item Reference',
		element: () => import('./member-type-item-ref.element.js'),
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
	},
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Member Type Item Repository',
		api: () => import('./member-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEMBER_TYPE_STORE_ALIAS,
		name: 'Member Type Item Store',
		api: UmbMemberTypeItemStore,
	},
];
