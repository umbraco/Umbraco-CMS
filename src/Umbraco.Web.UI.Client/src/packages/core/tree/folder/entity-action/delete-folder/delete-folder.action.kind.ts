import { UmbDeleteFolderEntityAction } from './delete-folder.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_GROUP_DELETE } from '@umbraco-cms/backoffice/action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Delete',
	matchKind: 'folderDelete',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'folderDelete',
		group: UMB_ACTION_GROUP_DELETE,
		api: UmbDeleteFolderEntityAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: '#actions_folderDelete',
			additionalOptions: true,
		},
	},
};
