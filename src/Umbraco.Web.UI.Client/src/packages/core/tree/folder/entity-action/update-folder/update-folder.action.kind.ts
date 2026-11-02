import { UmbUpdateFolderEntityAction } from './update-folder.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST,
	UMB_ENTITY_ACTION_GROUP_STRUCTURE,
} from '@umbraco-cms/backoffice/entity-action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Update',
	matchKind: 'folderUpdate',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		kind: 'folderUpdate',
		api: UmbUpdateFolderEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-edit',
			label: '#actions_folderRename',
			additionalOptions: true,
		},
	},
};
