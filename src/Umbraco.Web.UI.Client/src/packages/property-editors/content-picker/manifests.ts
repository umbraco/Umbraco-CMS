import { manifest as sourceManifest } from './config/source-content/manifests.js';
import { manifest as sourceTypeManifest } from './config/source-type/manifests.js';
import { manifest as schemaManifest } from './Umbraco.MultiNodeTreePicker.js';
import { manifests as dynamicRootManifests } from './dynamic-root/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ContentPicker',
	name: 'Content Picker Property Editor UI',
	element: () => import('./property-editor-ui-content-picker.element.js'),
	meta: {
		label: 'Content Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: 'Umbraco.MultiNodeTreePicker',
		supportsReadOnly: true,
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: 'Select the applicable types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [sourceManifest, sourceTypeManifest];

export const manifests: Array<UmbExtensionManifest> = [manifest, ...config, schemaManifest, ...dynamicRootManifests];
