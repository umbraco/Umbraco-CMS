import { UmbDefaultFilterActiveValueApi } from './default-filter-active-value.api.js';
import { UmbDefaultFilterActiveValueElement } from './default-filter-active-value.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.FilterActiveValue.Default',
		matchKind: 'default',
		matchType: 'filterActiveValue',
		manifest: {
			type: 'filterActiveValue',
			kind: 'default',
			element: UmbDefaultFilterActiveValueElement,
			api: UmbDefaultFilterActiveValueApi,
		},
	},
];
