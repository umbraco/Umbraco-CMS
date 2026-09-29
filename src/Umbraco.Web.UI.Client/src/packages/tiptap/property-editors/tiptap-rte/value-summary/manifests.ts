import { UMB_TIPTAP_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.Tiptap',
		name: 'Rich Text Editor Property Editor Value Summary',
		forValueType: UMB_TIPTAP_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
