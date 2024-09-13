import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.User.CreateOptions',
		name: 'User Create Options Modal',
		element: () => import('./user-create-options-modal.element.js'),
	},
];
