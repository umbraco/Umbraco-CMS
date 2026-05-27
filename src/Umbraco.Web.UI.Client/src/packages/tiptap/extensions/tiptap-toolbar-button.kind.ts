import { UmbTiptapToolbarButtonElement } from '../components/toolbar/tiptap-toolbar-button.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.Button',
	matchKind: 'button',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		element: UmbTiptapToolbarButtonElement,
	},
};
