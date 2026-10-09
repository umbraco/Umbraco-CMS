import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
		name: 'Tiptap Extensions Property Editor UI',
		element: () => import('./property-editor-ui-tiptap-extensions-configuration.element.js'),
		meta: {
			supportsVariantChange: true,
			label: 'Tiptap Extensions Configuration',
			icon: 'icon-autofill',
			group: '#propertyEditorUIGroups_common',
		},
	},
];
