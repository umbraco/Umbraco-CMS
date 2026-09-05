import { manifest as schemaManifest } from './Umbraco.DocumentPicker.Multiple.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DocumentPicker.Multiple',
		name: 'Multiple Document Picker Property Editor UI',
		element: () => import('./property-editor-ui-multiple-document-picker.element.js'),
		meta: {
			label: 'Multiple Document Picker',
			propertyEditorSchemaAlias: 'Umbraco.DocumentPicker.Multiple',
			icon: 'icon-documents',
			group: '#propertyEditorUIGroups_pickers',
			keywords: [
				'select',
				'page',
				'link',
				'reference',
				'related',
				'document',
				'target',
				'destination',
				'multiple',
				'several',
			],
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'allowedContentTypes',
						label: 'Accepted types',
						description: 'Limit to specific types',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentTypePicker',
						config: [{ alias: 'onlyPickDocumentTypes', value: true }],
						weight: 10,
					},
					{
						alias: 'validationLimit',
						label: 'Amount',
						description: 'Set a required range of documents',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
						config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
						weight: 20,
					},
					{
						alias: 'startNodeId',
						label: 'Start node',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentPicker',
						config: [{ alias: 'validationLimit', value: { min: 0, max: 1 } }],
						weight: 30,
					},
					{
						alias: 'dynamicRoot',
						label: 'Dynamic root',
						description: 'Resolve the start node from the content being edited, when no start node is set',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.DynamicRoot',
						weight: 40,
					},
				],
			},
		},
	},
	schemaManifest,
	...valueSummaryManifests,
];
