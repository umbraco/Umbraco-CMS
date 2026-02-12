import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.StyleMenu',
	matchKind: 'styleMenu',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		api: () => import('./style-menu.tiptap-toolbar-api.js'),
		element: () => import('../../components/toolbar/tiptap-toolbar-menu.element.js'),
	},
};
