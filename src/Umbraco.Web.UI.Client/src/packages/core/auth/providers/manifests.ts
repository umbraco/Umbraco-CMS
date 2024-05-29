import type { ManifestAuthProvider } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestAuthProvider> = [
	{
		type: 'authProvider',
		alias: 'Umb.AuthProviders.Umbraco',
		name: 'Umbraco login provider',
		forProviderName: 'Umbraco',
		weight: 1000,
		meta: {
			label: 'Umbraco',
			defaultView: {
				icon: 'icon-umbraco',
				look: 'primary',
			},
		},
	},
];
