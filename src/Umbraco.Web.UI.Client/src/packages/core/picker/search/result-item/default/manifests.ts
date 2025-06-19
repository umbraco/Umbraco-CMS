import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PickerSearchResultItem.Default',
		matchKind: 'default',
		matchType: 'pickerSearchResultItem',
		manifest: {
			type: 'pickerSearchResultItem',
			api: () => import('./default-picker-search-result-item.context.js'),
			element: () => import('./default-picker-search-result-item.element.js'),
		},
	},
];
