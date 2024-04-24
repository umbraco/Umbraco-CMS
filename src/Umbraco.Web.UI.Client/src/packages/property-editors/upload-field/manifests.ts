import { manifest as schemaManifest } from './Umbraco.UploadField.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.UploadField',
	name: 'Upload Field Property Editor UI',
	element: () => import('./property-editor-ui-upload-field.element.js'),
	meta: {
		label: 'Upload Field',
		propertyEditorSchemaAlias: 'Umbraco.UploadField',
		icon: 'icon-download-alt',
		group: 'common',
	},
};

export const manifests: Array<ManifestTypes> = [manifest, schemaManifest];
