import { UmbPropertyEditorUiTiptapStatusbarConfigurationElement } from './property-editor-ui-tiptap-statusbar-configuration.element.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.StatusbarConfiguration',
		name: 'Tiptap Statusbar Property Editor UI',
		element: UmbPropertyEditorUiTiptapStatusbarConfigurationElement,
		meta: {
			label: 'Tiptap Statusbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
