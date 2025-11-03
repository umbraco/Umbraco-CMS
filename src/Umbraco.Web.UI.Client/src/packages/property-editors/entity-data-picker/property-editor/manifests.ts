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
						alias: 'displayMode',
						label: 'Display Mode',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.EntityDataPicker.DisplayMode',
						weight: 100,
					},
				],
			},
		},
	},
	...schemaManifests,
	{
		type: 'entityDataPickerDisplayMode',
		alias: 'Umb.EntityDataPickerDisplayMode.MenuItem',
		name: 'Menu Item Entity Data Picker Display Mode',
		weight: 900,
		meta: {
			label: 'Menu Item',
			description: 'Description of Menu Item display mode.',
		},
	},
	{
		type: 'entityDataPickerDisplayMode',
		alias: 'Umb.EntityDataPickerDisplayMode.RefItem',
		name: 'Ref Item Entity Data Picker Display Mode',
		weight: 800,
		meta: {
			label: 'Reference Item',
			description: 'Description of Reference Item display mode.',
		},
	},
	{
		type: 'entityDataPickerDisplayMode',
		alias: 'Umb.EntityDataPickerDisplayMode.Card',
		name: 'Card Entity Data Picker Display Mode',
		weight: 700,
		meta: {
			label: 'Card',
			description: 'Description of Card display mode.',
		},
	},
];
