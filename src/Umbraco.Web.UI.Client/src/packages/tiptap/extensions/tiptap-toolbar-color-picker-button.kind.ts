import { UmbTiptapToolbarColorPickerButtonElement } from '../components/toolbar/tiptap-toolbar-color-picker-button.element.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.TiptapToolbar.ColorPickerButton',
	matchKind: 'colorPickerButton',
	matchType: 'tiptapToolbarExtension',
	manifest: {
		element: UmbTiptapToolbarColorPickerButtonElement,
	},
};
