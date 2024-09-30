import type { ManifestTiptapExtension } from '../tiptap-extension.js';

export const manifests: Array<ManifestTiptapExtension> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embed Tiptap Extension',
		api: () => import('./embedded-media.extension.js'),
		meta: {
			icon: 'icon-embed',
			label: '#general_embed',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Link',
		name: 'Link Tiptap Extension',
		api: () => import('./link.extension.js'),
		meta: {
			icon: 'icon-link',
			label: '#defaultdialogs_urlLinkPicker',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Figure',
		name: 'Figure Tiptap Extension',
		api: () => import('./figure.extension.js'),
		meta: {
			icon: 'icon-frame',
			label: 'Figure',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Image',
		name: 'Image Tiptap Extension',
		api: () => import('./image.extension.js'),
		meta: {
			icon: 'icon-picture',
			label: 'Image',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Subscript',
		name: 'Subscript Tiptap Extension',
		api: () => import('./subscript.extension.js'),
		meta: {
			icon: 'icon-subscript',
			label: 'Subscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Superscript',
		name: 'Superscript Tiptap Extension',
		api: () => import('./superscript.extension.js'),
		meta: {
			icon: 'icon-superscript',
			label: 'Superscript',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Table',
		name: 'Table Tiptap Extension',
		api: () => import('./table.extension.js'),
		meta: {
			icon: 'icon-table',
			label: 'Table',
			group: '#tiptap_extGroup_interactive',
		},
	},
	{
		type: 'tiptapExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Underline',
		name: 'Underline Tiptap Extension',
		api: () => import('./underline.extension.js'),
		meta: {
			icon: 'icon-underline',
			label: 'Underline',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.TextAlign',
		name: 'Text Align Tiptap Extension',
		api: () => import('./text-align.extension.js'),
		meta: {
			icon: 'icon-text-align-justify',
			label: 'Text Align',
			group: '#tiptap_extGroup_formatting',
		},
	},
];
