import { UmbUpdateFolderEntityAction } from './update-folder.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_GROUP_STRUCTURE } from '@umbraco-cms/backoffice/action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Update',
	matchKind: 'folderUpdate',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'folderUpdate',
		group: UMB_ACTION_GROUP_STRUCTURE,
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
