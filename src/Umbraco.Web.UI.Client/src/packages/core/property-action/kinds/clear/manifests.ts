import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '../default/index.js';

export const UMB_PROPERTY_ACTION_CLEAR_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PropertyAction.Clear',
	matchType: 'propertyAction',
	matchKind: 'clear',
	manifest: {
		...UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		api: () => import('../../common/clear/property-action-clear.controller.js'),
		meta: {
			icon: 'icon-trash',
			label: '#actions_clear',
		},
		conditions: [{ alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS }],
	},
};

export const manifest = UMB_PROPERTY_ACTION_CLEAR_KIND_MANIFEST;
