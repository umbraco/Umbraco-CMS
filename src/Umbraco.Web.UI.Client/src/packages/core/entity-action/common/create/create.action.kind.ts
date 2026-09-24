import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import { UmbCreateEntityAction } from './create.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_GROUP_CREATE } from '@umbraco-cms/backoffice/action';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Create',
	matchKind: 'create',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'create',
		group: UMB_ACTION_GROUP_CREATE,
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
