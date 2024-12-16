import { UMB_SCRIPT_ENTITY_TYPE } from '../entity.js';
import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS, UMB_SCRIPT_ITEM_REPOSITORY_ALIAS } from '../repository/constants.js';
import { manifests as createManifests } from './create/manifests.js';
import { manifests as renameManifests } from './rename/manifests.js';

export const UMB_DELETE_SCRIPT_ENTITY_ACTION_ALIAS = 'Umb.EntityAction.Script.Delete';

export const manifests: Array<UmbExtensionManifest> = [
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
	},
];
