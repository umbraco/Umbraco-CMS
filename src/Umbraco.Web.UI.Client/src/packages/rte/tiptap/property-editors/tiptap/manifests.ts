import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap',
		name: 'Rich Text Editor [Tiptap] Property Editor UI',
		element: () => import('./property-editor-ui-tiptap.element.js'),
		meta: {
			label: 'Rich Text Editor [Tiptap]',
			propertyEditorSchemaAlias: 'Umbraco.RichText',
			icon: 'icon-browser-window',
			group: 'richContent',
		},
	},
];
