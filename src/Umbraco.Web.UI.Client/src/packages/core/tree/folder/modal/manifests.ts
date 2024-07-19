import type { ManifestModal, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Update',
		name: 'Update Folder Modal',
		js: () => import('./folder-update-modal.element.js'),
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Folder.Create',
		name: 'Create Folder Modal',
		js: () => import('./folder-create-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...modals];
