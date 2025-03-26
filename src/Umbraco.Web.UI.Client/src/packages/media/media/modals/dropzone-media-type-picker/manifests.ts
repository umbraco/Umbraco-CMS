export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Dropzone.MediaTypePicker',
		name: 'Dropzone Media Type Picker Modal',
		element: () => import('./dropzone-media-type-picker-modal.element.js'),
	},
];
