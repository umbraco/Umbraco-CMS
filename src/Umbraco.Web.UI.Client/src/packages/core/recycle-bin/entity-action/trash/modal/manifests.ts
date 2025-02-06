export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Trash.Confirm',
		name: 'Trash Confirm Modal',
		element: () => import('./trash-confirm-modal.element.js'),
	},
];
