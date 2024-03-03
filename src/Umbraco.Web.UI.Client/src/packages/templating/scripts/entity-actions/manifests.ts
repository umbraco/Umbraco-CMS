import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../repository/item/index.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Delete';

const scriptViewActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Delete Script Entity Action',
		kind: 'delete',
		meta: {
			detailRepositoryAlias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...scriptViewActions, ...createManifests, ...renameManifests];
