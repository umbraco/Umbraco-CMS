export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CurrentUserExternalLogin',
		name: 'External Login Modal',
		element: () => import('./modals/external-login-modal.element.js'),
	},
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUser.App.ExternalLoginProviders',
		name: 'External Login Providers Current User App',
		weight: 700,
		api: () => import('./configure-external-login-providers-action.js'),
		meta: {
			label: '#defaultdialogs_externalLoginProviders',
			icon: 'icon-lock',
			look: 'secondary',
		},
		conditions: [
			{
				alias: 'Umb.Condition.User.AllowExternalLoginAction',
			},
		],
	},
];
