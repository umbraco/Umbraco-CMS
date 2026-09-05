import { UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST } from '../trash/constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_TRASH_FOLDER_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.TrashFolder',
	matchKind: 'trashFolder',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'trashFolder',
		api: () => import('./trash-folder.action.js'),
		weight: 1150,
		meta: {
			icon: 'icon-trash',
			label: '#actions_trash',
			additionalOptions: true,
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_TRASH_FOLDER_KIND_MANIFEST;
