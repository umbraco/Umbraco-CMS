import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import { UMB_ENTITY_ACTION_GROUP_STRUCTURE } from '../../action-groups/constants.js';
import { UmbDuplicateEntityAction } from './duplicate.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Duplicate',
	matchKind: 'duplicate',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		kind: 'duplicate',
		api: UmbDuplicateEntityAction,
		weight: 650,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_copy',
			additionalOptions: true,
			treeRepositoryAlias: '',
			duplicateRepositoryAlias: '',
		},
	},
};
