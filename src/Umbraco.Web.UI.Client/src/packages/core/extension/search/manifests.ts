import { UMB_EXTENSION_ENTITY_TYPE } from '../entity.js';
import { UMB_EXTENSION_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'searchProvider',
		alias: UMB_EXTENSION_SEARCH_PROVIDER_ALIAS,
		name: 'Extension Search Provider',
		api: () => import('./search-provider.js'),
		weight: 600,
	},
	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.Extension',
		name: 'Extension Picker Search Result Item',
		forEntityTypes: [UMB_EXTENSION_ENTITY_TYPE],
	},
];
