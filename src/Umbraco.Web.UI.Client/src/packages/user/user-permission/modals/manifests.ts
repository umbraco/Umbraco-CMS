import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.EntityUserPermissionSettings',
		name: 'Entity User Permission Settings Modal',
		js: () => import('./settings/entity-user-permission-settings-modal.element.js'),
	},
];
