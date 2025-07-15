import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Member',
		name: 'Member Picker Search Result Item',
		element: () => import('./member-picker-search-result-item.element.js'),
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
	},
];
