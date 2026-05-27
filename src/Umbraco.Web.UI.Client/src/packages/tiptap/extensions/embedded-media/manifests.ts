import UmbTiptapEmbeddedMediaExtensionApi from './embedded-media.tiptap-api.js';
import UmbTiptapToolbarEmbeddedMediaExtensionApi from './embedded-media.tiptap-toolbar-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Embed',
		name: 'Embedded Media Tiptap Extension',
		api: UmbTiptapEmbeddedMediaExtensionApi,
		meta: {
			icon: 'icon-embed',
			label: '#general_embed',
			group: '#tiptap_extGroup_media',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.EmbeddedMedia',
		name: 'Embedded Media Tiptap Toolbar Extension',
		api: UmbTiptapToolbarEmbeddedMediaExtensionApi,
		forExtensions: ['Umb.Tiptap.Embed'],
		meta: {
			alias: 'umbEmbeddedMedia',
			icon: 'icon-embed',
			label: '#general_embed',
		},
	},
];
