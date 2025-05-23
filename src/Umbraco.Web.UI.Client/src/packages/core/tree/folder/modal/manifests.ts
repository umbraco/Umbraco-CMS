export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Update',
		name: 'Update Folder Modal',
		element: () => import('./folder-update-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Create',
		name: 'Create Folder Modal',
		element: () => import('./folder-create-modal.element.js'),
	},
];
