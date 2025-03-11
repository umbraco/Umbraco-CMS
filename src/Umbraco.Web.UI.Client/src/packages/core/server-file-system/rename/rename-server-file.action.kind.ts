import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../entity-action/default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.ServerFile.Rename',
	matchKind: 'renameServerFile',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'renameServerFile',
		api: () => import('./rename-server-file.action.js'),
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
