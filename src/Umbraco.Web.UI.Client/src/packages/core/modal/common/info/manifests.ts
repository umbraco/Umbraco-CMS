import UmbInfoModalElement from './info-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Info',
		name: 'Info Modal',
		element: UmbInfoModalElement,
	},
];
