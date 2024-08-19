import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
	UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
} from '../repository/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move-to/manifests.js';
import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as exportManifests } from './export/manifests.js';
import { manifests as importManifests } from './import/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'delete',
		alias: 'Umb.EntityAction.DocumentType.Delete',
		name: 'Delete Document-Type Entity Action',
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
		meta: {
			itemRepositoryAlias: UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
			detailRepositoryAlias: UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<ManifestTypes> = [
	...entityActions,
	...createManifests,
	...moveManifests,
	...duplicateManifests,
	...exportManifests,
	...importManifests,
];
