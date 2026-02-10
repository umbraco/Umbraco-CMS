import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.ActionButton',
	matchKind: 'actionButton',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		element: () => import('../components/toolbar/tiptap-toolbar-button-action.element.js'),
	},
};
