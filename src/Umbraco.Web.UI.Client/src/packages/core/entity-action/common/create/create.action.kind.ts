import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbCreateEntityAction } from './create.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Create',
	matchKind: 'create',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
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
