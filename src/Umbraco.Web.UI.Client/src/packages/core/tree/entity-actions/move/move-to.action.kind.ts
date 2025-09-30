import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.MoveTo',
	matchKind: 'moveTo',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'moveTo',
		api: () => import('./move-to.action.js'),
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_move',
			additionalOptions: true,
			treeRepositoryAlias: '',
			moveRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
