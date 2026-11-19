import { UmbDuplicateToEntityAction } from './duplicate-to.action.js';
import {
	UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST,
	UMB_ENTITY_ACTION_GROUP_STRUCTURE,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.DuplicateTo',
	matchKind: 'duplicateTo',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		kind: 'duplicateTo',
		api: UmbDuplicateToEntityAction,
		weight: 710,
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
