import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDuplicateToEntityAction } from './duplicate-to.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.DuplicateTo',
	matchKind: 'duplicateTo',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'duplicateTo',
		api: UmbDuplicateToEntityAction,
		weight: 600,
		forEntityTypes: [],
		meta: {
			icon: 'icon-split',
			label: '#actions_copyTo',
			additionalOptions: true,
			treeRepositoryAlias: '',
			duplicateRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
