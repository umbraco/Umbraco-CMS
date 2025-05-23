import type { ManifestAuthProvider } from '../auth-provider.extension.js';

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
