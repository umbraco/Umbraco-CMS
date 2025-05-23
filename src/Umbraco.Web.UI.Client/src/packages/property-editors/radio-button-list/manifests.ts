import { manifest as radioButtonListSchemaManifest } from './Umbraco.RadioButtonList.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.RadioButtonList',
		name: 'Radio Button List Property Editor UI',
		element: () => import('./property-editor-ui-radio-button-list.element.js'),
		meta: {
			label: 'Radio Button List',
			propertyEditorSchemaAlias: 'Umbraco.RadioButtonList',
			icon: 'icon-target',
			group: 'lists',
			supportsReadOnly: true,
		},
	},
	radioButtonListSchemaManifest,
];
