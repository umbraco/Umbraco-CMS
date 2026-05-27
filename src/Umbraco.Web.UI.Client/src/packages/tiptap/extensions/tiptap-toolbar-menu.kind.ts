import { UmbTiptapToolbarMenuElement } from '../components/toolbar/tiptap-toolbar-menu.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.Menu',
	matchKind: 'menu',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		element: UmbTiptapToolbarMenuElement,
	},
};
