import { UmbDeleteFolderEntityAction } from './delete-folder.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST,
	UMB_ENTITY_ACTION_GROUP_STRUCTURE,
} from '@umbraco-cms/backoffice/entity-action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Folder.Delete',
	matchKind: 'folderDelete',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		kind: 'folderDelete',
		api: UmbDeleteFolderEntityAction,
		weight: 799,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: '#actions_folderDelete',
			additionalOptions: true,
		},
	},
};
