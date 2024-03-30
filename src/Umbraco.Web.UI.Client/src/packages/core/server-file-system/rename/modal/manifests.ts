import type { ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RENAME_MODAL_ALIAS = 'Umb.Modal.Rename';

export const manifests: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: UMB_RENAME_MODAL_ALIAS,
		name: 'Rename Modal',
		js: () => import('./rename-modal.element.js'),
	},
];
