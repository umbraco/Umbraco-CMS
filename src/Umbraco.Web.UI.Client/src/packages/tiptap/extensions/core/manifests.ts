import UmbTiptapRichTextEssentialsExtensionApi from './rich-text-essentials.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.RichTextEssentials',
		name: 'Rich Text Essentials Tiptap Extension',
		api: UmbTiptapRichTextEssentialsExtensionApi,
		weight: 1000,
		meta: {
			icon: 'icon-browser-window',
			label: 'Rich Text Essentials',
			group: '#tiptap_extGroup_formatting',
			description: 'This is a core extension, it is always enabled by default.',
		},
	},
];
