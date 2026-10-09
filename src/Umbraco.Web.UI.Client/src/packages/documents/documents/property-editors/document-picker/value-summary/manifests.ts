import { UMB_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.DocumentPicker',
		name: 'Document Picker Property Editor Value Summary',
		forValueType: UMB_DOCUMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.js'),
		valueResolver: () => import('./value-summary.js'),
	},
];
