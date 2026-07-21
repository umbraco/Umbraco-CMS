import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbRestoreFromRecycleBinEntityAction } from './restore-from-recycle-bin.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.RecycleBin.Restore',
	matchKind: 'restoreFromRecycleBin',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'restoreFromRecycleBin',
		api: UmbRestoreFromRecycleBinEntityAction,
		weight: 100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-undo',
			label: '#actions_restore',
			pickerModal: '',
			additionalOptions: true,
		},
	},
};
