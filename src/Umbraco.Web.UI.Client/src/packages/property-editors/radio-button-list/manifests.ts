import { manifest as radioButtonListSchemaManifest } from './Umbraco.RadioButtonList.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
		},
	},
	radioButtonListSchemaManifest,
];
