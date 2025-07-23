import { UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.MemberType.Delete',
		name: 'Delete Member Type Entity Action',
		forEntityTypes: [UMB_MEMBER_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_MEMBER_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_MEMBER_TYPE_ITEM_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
	...duplicateManifests,
];
