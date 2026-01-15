import { manifests as kindManifests } from './kinds/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.Clear',
		name: 'Clear Property Action',
		api: () => import('./common/clear/property-action-clear.controller.js'),
		forPropertyEditorUis: [],
		meta: {
			icon: 'icon-trash',
			label: 'Clear',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	...kindManifests,
];
