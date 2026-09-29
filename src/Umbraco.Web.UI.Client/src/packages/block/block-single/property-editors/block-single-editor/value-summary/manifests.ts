import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.BlockSingle',
		name: 'Single Block Property Editor Value Summary',
		forValueType: UMB_BLOCK_SINGLE_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
