import UmbFolderModalElement from './folder-update-modal.element.js';
import UmbFolderCreateModalElement from './folder-create-modal.element.js';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Update',
		name: 'Update Folder Modal',
		element: UmbFolderModalElement,
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Create',
		name: 'Create Folder Modal',
		element: UmbFolderCreateModalElement,
	},
];
