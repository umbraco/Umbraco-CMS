import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbEmptyRecycleBinEntityAction } from './empty-recycle-bin.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.RecycleBin.Empty',
	matchKind: 'emptyRecycleBin',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'emptyRecycleBin',
		api: UmbEmptyRecycleBinEntityAction,
		weight: 100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash-empty',
			label: '#actions_emptyrecyclebin',
			additionalOptions: true,
		},
	},
};
