// eslint-disable-next-line local-rules/no-relative-import-to-import-map-module
import { manifests as tiptapManifests } from './tiptap/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...tiptapManifests,
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ToolbarConfiguration',
		name: 'Tiptap Toolbar Property Editor UI',
		js: () => import('./property-editor-ui-tiptap-toolbar-configuration.element.js'),
		meta: {
			label: 'Tiptap Toolbar Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tiptap.ExtensionsConfiguration',
		name: 'Tiptap Extensions Property Editor UI',
		js: () => import('./property-editor-ui-tiptap-extensions-configuration.element.js'),
		meta: {
			label: 'Tiptap Extensions Configuration',
			icon: 'icon-autofill',
			group: 'common',
		},
	},
];
