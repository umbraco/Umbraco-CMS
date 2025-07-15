export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.EntityUserPermissionSettings',
		name: 'Entity User Permission Settings Modal',
		js: () => import('./entity-user-permission-settings-modal.element.js'),
	},
];
