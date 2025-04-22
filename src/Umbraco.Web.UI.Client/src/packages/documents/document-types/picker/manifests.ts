import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.DocumentType',
		name: 'Document Type Picker Search Result Item',
		element: () => import('./document-type-picker-search-result-item.element.js'),
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
	},
];
