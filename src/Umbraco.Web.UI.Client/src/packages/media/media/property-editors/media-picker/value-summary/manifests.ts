import { UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.MediaPicker',
		name: 'Media Picker Property Editor Value Summary',
		forValueType: UMB_MEDIA_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
		valueResolver: () => import('./value-summary.resolver.js'),
	},
];
