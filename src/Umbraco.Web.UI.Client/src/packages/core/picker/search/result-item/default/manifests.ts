import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDefaultPickerSearchResultItemContext } from './default-picker-search-result-item.context.js';
import { UmbDefaultPickerSearchResultItemElement } from './default-picker-search-result-item.element.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PickerSearchResultItem.Default',
		matchKind: 'default',
		matchType: 'pickerSearchResultItem',
		manifest: {
			type: 'pickerSearchResultItem',
			api: UmbDefaultPickerSearchResultItemContext,
			element: UmbDefaultPickerSearchResultItemElement,
		},
	},
];
