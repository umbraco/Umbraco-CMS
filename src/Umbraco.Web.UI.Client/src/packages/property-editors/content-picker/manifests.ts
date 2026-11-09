import { manifest as sourceManifest } from './config/source-content/manifests.js';
import { manifest as sourceTypeManifest } from './config/source-type/manifests.js';
import { manifest as schemaManifest } from './Umbraco.MultiNodeTreePicker.js';
import { manifests as dynamicRootManifests } from './dynamic-root/manifests.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const propertyEditorUiAlias = 'Umb.PropertyEditorUi.ContentPicker';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: propertyEditorUiAlias,
	name: 'Content Picker Property Editor UI',
	element: () => import('./property-editor-ui-content-picker.element.js'),
	meta: {
		supportsVariantChange: true,
		label: 'Content Picker',
		icon: 'icon-page-add',
		group: '#propertyEditorUIGroups_pickers',
		keywords: ['select', 'page', 'node', 'reference', 'related', 'link', 'pages', 'content'],
		propertyEditorSchemaAlias: 'Umbraco.MultiNodeTreePicker',
		supportsReadOnly: true,
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [sourceManifest, sourceTypeManifest];

export const manifests: Array<UmbExtensionManifest> = [
	manifest,
	...config,
	schemaManifest,
	...dynamicRootManifests,
	...valueSummaryManifests,
	{
		type: 'propertyAction',
		kind: 'clear',
		alias: 'Umb.PropertyAction.ContentPicker.Clear',
		name: 'Clear Content Picker Property Action',
		forPropertyEditorUis: [propertyEditorUiAlias],
	},
];
