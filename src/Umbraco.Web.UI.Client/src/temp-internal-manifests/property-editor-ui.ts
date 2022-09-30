import type { ManifestTypes } from '../core/models';

export const manifests: Array<ManifestTypes & { loader: () => Promise<object | HTMLElement> }> = [
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Text',
		name: 'Text Property Editor UI',
		loader: () => import('../backoffice/property-editors/text/property-editor-text.element'),
		meta: {
			label: 'Text',
			icon: 'umb:edit',
			group: 'Common',
			propertyEditor: 'Umbraco.TextBox',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Textarea',
		name: 'Textarea Property Editor UI',
		elementName: 'umb-property-editor-textarea',
		loader: () => import('../backoffice/property-editors/textarea/property-editor-textarea.element'),
		meta: {
			label: 'Textarea',
			icon: 'umb:edit',
			group: 'Common',
			propertyEditor: 'Umbraco.TextArea',
			config: {
				properties: [
					{
						alias: 'rows',
						label: 'Number of rows',
						description: 'If empty - 10 rows would be set as the default value',
						propertyEditorUI: 'Umb.PropertyEditorUI.Number',
					},
				],
			},
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContextExample',
		name: 'Context Example Property Editor UI',
		loader: () => import('../backoffice/property-editors/context-example/property-editor-context-example.element'),
		meta: {
			label: 'Context Example',
			icon: 'umb:favorite',
			group: 'Common',
			propertyEditor: 'Umbraco.Custom',
			config: {
				properties: [
					{
						label: 'Some Configuration',
						alias: 'someConfiguration',
						propertyEditorUI: 'Umb.PropertyEditorUI.Text',
					},
				],
				defaultData: [
					{
						alias: 'someConfiguration',
						value: 'Some default value',
					},
				],
			},
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Number',
		name: 'Number Property Editor UI',
		loader: () => import('../backoffice/property-editors/number/property-editor-number.element'),
		meta: {
			label: 'Number',
			icon: 'umb:autofill',
			group: 'Common',
			propertyEditor: 'Umbraco.Integer',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.ContentPicker',
		name: 'Content Picker Property Editor UI',
		elementName: 'umb-property-editor-content-picker',
		loader: () => import('../backoffice/property-editors/content-picker/property-editor-content-picker.element'),
		meta: {
			label: 'Content Picker',
			propertyEditor: 'Umbraco.ContentPicker',
			icon: 'umb:document',
			group: 'Common',
		},
	},
];
