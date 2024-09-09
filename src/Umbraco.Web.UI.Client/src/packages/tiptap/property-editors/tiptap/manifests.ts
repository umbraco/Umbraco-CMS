import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap',
		name: 'Tiptap Property Editor UI',
		element: () => import('./property-editor-ui-tiptap.element.js'),
		meta: {
			label: 'Tiptap Editor',
			propertyEditorSchemaAlias: 'Umbraco.Tiptap',
			icon: 'icon-document',
			group: 'richText',
		},
	},
];
