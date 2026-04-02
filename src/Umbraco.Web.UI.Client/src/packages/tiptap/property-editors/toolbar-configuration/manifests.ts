import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
		name: 'Tiptap Toolbar Property Editor UI',
		element: () => import('./property-editor-ui-tiptap-toolbar-configuration.element.js'),
		meta: {
			label: 'Tiptap Toolbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
