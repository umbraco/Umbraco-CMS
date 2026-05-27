import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.StyleMenu',
	matchKind: 'styleMenu',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		api: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarStyleMenuApi })),
		element: () => import('../extension-apis.bundle.js').then((m) => ({ default: m.UmbTiptapToolbarMenuElement })),
	},
};
