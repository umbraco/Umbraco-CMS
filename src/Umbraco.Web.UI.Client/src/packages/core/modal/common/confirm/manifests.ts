import UmbConfirmModalElement from './confirm-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Confirm',
		name: 'Confirm Modal',
		element: UmbConfirmModalElement,
	},
];
