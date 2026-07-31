import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Element',
		name: 'Element Picker Search Result Item',
		element: () => import('./element-picker-search-result-item.element.js'),
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
	},
];
