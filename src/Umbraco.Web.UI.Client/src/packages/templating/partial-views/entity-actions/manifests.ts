import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from '../entity.js';
import { UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS } from '../repository/item/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const partialViewActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.PartialView.Delete',
		name: 'Delete Partial View Entity Action',
		kind: 'delete',
		forEntityTypes: [UMB_PARTIAL_VIEW_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_PARTIAL_VIEW_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_PARTIAL_VIEW_ITEM_REPOSITORY_ALIAS,
		},
	},
];

export const manifests = [...partialViewActions, ...createManifests, ...renameManifests];
