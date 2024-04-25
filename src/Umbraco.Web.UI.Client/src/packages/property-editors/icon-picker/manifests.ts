import { manifest as schemaManifest } from './Umbraco.IconPicker.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.IconPicker',
	name: 'Icon Picker Property Editor UI',
	element: () => import('./property-editor-ui-icon-picker.element.js'),
	meta: {
		label: 'Icon Picker',
		propertyEditorSchemaAlias: 'Umbraco.IconPicker',
		icon: 'icon-autofill',
		group: 'common',
	},
};

export const manifests: Array<ManifestTypes> = [manifest, schemaManifest];
