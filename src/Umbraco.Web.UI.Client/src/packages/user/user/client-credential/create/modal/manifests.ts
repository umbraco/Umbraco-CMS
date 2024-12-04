export const UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS = 'Umb.Modal.User.ClientCredential.Create';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS,
		name: 'Create User Client Credential Modal',
		element: () => import('./create-user-client-credential-modal.element.js'),
	},
];
