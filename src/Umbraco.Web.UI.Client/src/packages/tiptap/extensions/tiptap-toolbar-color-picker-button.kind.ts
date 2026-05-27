import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.ColorPickerButton',
	matchKind: 'colorPickerButton',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		element: () =>
			import('./extension-apis.bundle.js').then((m) => ({
				default: m.UmbTiptapToolbarColorPickerButtonElement,
			})),
	},
};
