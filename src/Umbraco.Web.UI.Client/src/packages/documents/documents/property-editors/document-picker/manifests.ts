import { manifest as schemaManifest } from './Umbraco.ContentPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';
import { UMB_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS } from '@umbraco-cms/backoffice/content';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DocumentPicker',
		name: 'Document Picker Property Editor UI',
		element: () => import('./property-editor-ui-document-picker.element.js'),
		meta: {
			supportsVariantChange: true,
			label: 'Single Document Picker',
			propertyEditorSchemaAlias: 'Umbraco.ContentPicker',
			icon: 'icon-document',
			group: '#propertyEditorUIGroups_pickers',
			keywords: ['select', 'page', 'link', 'reference', 'related', 'document', 'target', 'destination'],
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
						alias: 'startNodeId',
						label: 'Start node',
						description: '',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentPicker',
						config: [
							{
								alias: 'validationLimit',
								value: { min: 0, max: 1 },
							},
						],
						weight: 20,
					},
					{
						alias: 'dynamicRoot',
						label: 'Dynamic root',
						description: 'Resolve the start node from the content being edited, when no start node is set',
						propertyEditorUiAlias: UMB_DYNAMIC_ROOT_PROPERTY_EDITOR_UI_ALIAS,
						weight: 30,
					},
				],
			},
		},
	},
	schemaManifest,
	...valueSummaryManifests,
];
