import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Document',
		name: 'Document Picker Search Result Item',
		element: () => import('./document-picker-search-result-item.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
