export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypePickerFlow',
		name: 'Data Type Picker Flow Modal',
		element: () => import('./data-type-picker-flow/data-type-picker-flow-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypePickerFlowDataTypePicker',
		name: 'Data Type Picker Flow UI Picker Modal',
		element: () => import('./data-type-picker-flow/data-type-picker-flow-data-type-picker-modal.element.js'),
	},
];
