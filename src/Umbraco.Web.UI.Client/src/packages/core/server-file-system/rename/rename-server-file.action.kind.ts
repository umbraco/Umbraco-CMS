import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import UmbRenameEntityAction from './rename-server-file.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.ServerFile.Rename',
	matchKind: 'renameServerFile',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'renameServerFile',
		api: UmbRenameEntityAction,
		weight: 200,
		forEntityTypes: [],
		meta: {
			icon: 'icon-edit',
			label: '#actions_rename',
			additionalOptions: true,
			renameRepositoryAlias: '',
			itemRepositoryAlias: '',
		},
	},
};
