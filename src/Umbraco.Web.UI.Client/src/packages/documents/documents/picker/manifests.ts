import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	{
		type: 'pickerSearchResultItem',
		alias: 'Umb.PickerSearchResultItem.Document',
		name: 'Document Picker Search Result Item',
		element: () => import('./document-picker-search-result-item.element.js'),
	},
];
