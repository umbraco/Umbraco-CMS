import { UmbPropertyEditorUiTiptapExtensionsConfigurationElement } from './property-editor-ui-tiptap-extensions-configuration.element.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
		name: 'Tiptap Extensions Property Editor UI',
		element: UmbPropertyEditorUiTiptapExtensionsConfigurationElement,
		meta: {
			label: 'Tiptap Extensions Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
