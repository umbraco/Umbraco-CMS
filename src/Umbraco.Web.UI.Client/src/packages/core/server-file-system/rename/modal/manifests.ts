import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_SERVER_FILE_MODAL_ALIAS = 'Umb.Modal.ServerFile.Rename';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_RENAME_SERVER_FILE_MODAL_ALIAS,
		name: 'Rename Server File Modal',
		element: () => import('./rename-server-file-modal.element.js'),
	},
];
