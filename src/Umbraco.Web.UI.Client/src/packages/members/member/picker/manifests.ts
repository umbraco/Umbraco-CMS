import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Member',
		name: 'Member Picker Search Result Item',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
	},
];
