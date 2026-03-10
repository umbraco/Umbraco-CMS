import { manifests as schemaManifests } from './Umbraco.EntityDataPicker.js';
import { UMB_PICKER_DATA_SOURCE_TYPE } from '@umbraco-cms/backoffice/picker-data-source';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.EntityDataPicker',
		name: 'Entity Data Picker Property Editor UI',
		element: () => import('./entity-data-picker-property-editor-ui.element.js'),
		meta: {
			label: 'Entity Data Picker',
			icon: 'icon-page-add',
			group: 'pickers',
			propertyEditorSchemaAlias: 'Umbraco.EntityDataPicker',
			supportsReadOnly: true,
			supportsDataSource: {
				enabled: true,
				forDataSourceTypes: [UMB_PICKER_DATA_SOURCE_TYPE],
			},
			settings: {
				properties: [
					{
						alias: 'pickerViews',
						label: 'Picker Views',
						description:
							'Configure which views are available in the picker. Only applicable for collection-based data sources.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.EntityDataPicker.PickerViewsConfiguration',
					},
				],
			},
		},
	},
	...schemaManifests,
];
