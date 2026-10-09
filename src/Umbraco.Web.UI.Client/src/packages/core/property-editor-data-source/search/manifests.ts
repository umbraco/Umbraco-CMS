import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE } from '../entity.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS } from './constants.js';
import { UmbPropertyEditorDataSourceSearchProvider } from './search-provider.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'searchProvider',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_SEARCH_PROVIDER_ALIAS,
		name: 'Property Editor Data Source Search Provider',
		api: UmbPropertyEditorDataSourceSearchProvider,
		weight: 600,
	},

	{
		type: 'pickerSearchResultItem',
		kind: 'default',
		alias: 'Umb.PickerSearchResultItem.PropertyEditorDataSource',
		name: 'Property Editor Data Source Picker Search Result Item',
		forEntityTypes: [UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE],
	},
];
