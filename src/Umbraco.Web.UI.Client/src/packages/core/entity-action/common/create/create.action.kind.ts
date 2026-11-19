import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import { UMB_ENTITY_ACTION_GROUP_CREATE } from '../../action-groups/constants.js';
import { UmbCreateEntityAction } from './create.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Create',
	matchKind: 'create',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_CREATE,
		kind: 'create',
		api: UmbCreateEntityAction,
		weight: 1200,
		forEntityTypes: [],
		meta: {
			icon: 'icon-add',
			label: '#actions_createFor',
			additionalOptions: true,
		},
	},
};
