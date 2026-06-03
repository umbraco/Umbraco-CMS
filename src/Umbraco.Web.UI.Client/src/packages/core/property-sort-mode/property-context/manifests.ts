import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertySortModeContext } from './property-sort-mode.context.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyContext.SortMode',
		matchKind: 'sortMode',
		matchType: 'propertyContext',
		manifest: {
			type: 'propertyContext',
			kind: 'sortMode',
			api: UmbPropertySortModeContext,
			weight: 1300,
		},
	},
];
