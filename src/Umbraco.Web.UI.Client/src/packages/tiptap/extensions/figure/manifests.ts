import UmbTiptapFigureExtensionApi from './figure.tiptap-api.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Figure',
		name: 'Figure Tiptap Extension',
		api: UmbTiptapFigureExtensionApi,
		meta: {
			icon: 'icon-frame',
			label: 'Figure',
			group: '#tiptap_extGroup_media',
		},
	},
];
