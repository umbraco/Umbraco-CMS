import { UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.ImageCropper',
		name: 'Image Cropper Property Editor Value Summary',
		forValueType: UMB_IMAGE_CROPPER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
