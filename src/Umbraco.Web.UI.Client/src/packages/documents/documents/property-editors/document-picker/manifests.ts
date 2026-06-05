import { manifest as schemaManifest } from './Umbraco.ContentPicker.js';
import { manifest as allowedDocumentTypesManifest } from './allowed-document-types/manifests.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DocumentPicker',
		name: 'Document Picker Property Editor UI',
		element: () => import('./property-editor-ui-document-picker.element.js'),
		meta: {
			label: 'Document Picker',
			propertyEditorSchemaAlias: 'Umbraco.ContentPicker',
			icon: 'icon-document',
			group: '#propertyEditorUIGroups_pickers',
			keywords: ['select', 'page', 'link', 'reference', 'related', 'document', 'target', 'destination'],
			supportsReadOnly: true,
			settings: {
				properties: [
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
					},
					{
						alias: 'allowedContentTypes',
						label: 'Allow items of type',
						description: 'Select the applicable document types',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentPicker.AllowedDocumentTypes',
						weight: 120,
					},
				],
			},
		},
	},
	schemaManifest,
	allowedDocumentTypesManifest,
	...valueSummaryManifests,
];
