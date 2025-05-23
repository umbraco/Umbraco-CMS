import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.DuplicateTo',
	matchKind: 'duplicateTo',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'duplicateTo',
		api: () => import('./duplicate-to.action.js'),
		weight: 600,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_copyTo',
			additionalOptions: true,
			treeRepositoryAlias: '',
			duplicateRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
