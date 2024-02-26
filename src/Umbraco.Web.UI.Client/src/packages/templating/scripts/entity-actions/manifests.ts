import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS } from '../repository/index.js';
import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';
import { UmbDeleteEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Delete';

const scriptViewActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Delete Script Entity Action',
		api: UmbDeleteEntityAction,
		meta: {
			icon: 'icon-trash',
			label: 'Delete',
			repositoryAlias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		},
	},
];

export const manifests = [...scriptViewActions, ...createManifests, ...renameManifests];
