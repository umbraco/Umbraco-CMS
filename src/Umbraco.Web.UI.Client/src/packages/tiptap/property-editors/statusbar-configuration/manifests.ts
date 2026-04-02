import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.StatusbarConfiguration',
		name: 'Tiptap Statusbar Property Editor UI',
		element: () => import('./property-editor-ui-tiptap-statusbar-configuration.element.js'),
		meta: {
			label: 'Tiptap Statusbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
