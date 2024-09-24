import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../../entity-action/default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.RecycleBin.Restore',
	matchKind: 'restoreFromRecycleBin',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'restoreFromRecycleBin',
		api: () => import('./restore-from-recycle-bin.action.js'),
		weight: 100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-undo',
			label: 'Restore',
			pickerModal: '',
			additionalOptions: true,
		},
	},
};
