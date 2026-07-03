import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS, UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../repository/constants.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Delete';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...createManifests,
	...renameManifests,
	{
		type: 'entityAction',
		kind: 'delete',
		alias: UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS,
		name: 'Delete Script Entity Action',
		forEntityTypes: [UMB_SCRIPT_ENTITY_TYPE],
		meta: {
			detailRepositoryAlias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
			itemRepositoryAlias: UMB_SCRIPT_ITEM_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
