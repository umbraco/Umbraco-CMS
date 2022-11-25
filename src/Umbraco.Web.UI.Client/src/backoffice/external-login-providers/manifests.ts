import type { ManifestExternalLoginProvider, ManifestWithLoader } from '@umbraco-cms/models';

export const manifests: Array<ManifestWithLoader<ManifestExternalLoginProvider>> = [
	{
		type: 'externalLoginProvider',
		alias: 'Umb.ExternalLoginProvider.Test',
		name: 'Test External Login Provider',
		elementName: 'umb-external-login-provider-test',
		loader: () => import('./external-login-provider-test.element'),
		weight: 600,
		meta: {
			label: 'Test External Login Provider',
			pathname: 'test/test/test',
		},
	},
];
