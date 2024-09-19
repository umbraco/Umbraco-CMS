export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.CreateOptions',
		name: 'User Create Options Modal',
		element: () => import('./user-create-options-modal.element.js'),
	},
];
