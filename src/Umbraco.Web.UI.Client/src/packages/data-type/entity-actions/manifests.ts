import {
	UMB_DATA_TYPE_ENTITY_TYPE,
	UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
	UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
	UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS,
} from '../constants.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'deleteWithRelation',
		alias: 'Umb.EntityAction.DataType.Delete',
		name: 'Delete Data Type Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
			referenceRepositoryAlias: UMB_DATA_TYPE_REFERENCE_REPOSITORY_ALIAS,
		},
	},
	...createManifests,
	...moveManifests,
	...duplicateManifests,
];
