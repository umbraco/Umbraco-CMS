import { UMB_DATA_TYPE_ENTITY_TYPE } from '../entity.js';
import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/index.js';
import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS } from '../repository/item/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DataType.Delete',
		name: 'Delete Data Type Entity Action',
		forEntityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [
	...entityActions,
	...createManifests,
	...moveManifests,
	...duplicateManifests,
];
