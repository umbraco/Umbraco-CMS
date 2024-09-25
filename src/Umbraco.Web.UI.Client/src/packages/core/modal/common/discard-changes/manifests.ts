export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DiscardChanges',
		name: 'Discard Changes Modal',
		element: () => import('./discard-changes-modal.element.js'),
	},
];
