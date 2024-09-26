export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.MediaCaptionAltText',
		name: 'Media Caption Alt Text',
		element: () => import('./media-caption-alt-text-modal.element.js'),
	},
];
