import { UmbTiptapStatusbarElementPathElement } from './element-path.tiptap-statusbar-element.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapStatusbarExtension',
		alias: 'Umb.Tiptap.Statusbar.ElementPath',
		name: 'Element Path Tiptap Statusbar Extension',
		element: UmbTiptapStatusbarElementPathElement,
		meta: {
			alias: 'elementPath',
			icon: 'icon-map-alt',
			label: 'Element Path',
		},
	},
];
