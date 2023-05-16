// TODO: could these be renamed as login providers?
import type { ManifestExternalLoginProvider } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestExternalLoginProvider> = [
	{
		type: 'externalLoginProvider',
		alias: 'Umb.ExternalLoginProvider.Test',
		name: 'Test External Login Provider',
		elementName: 'umb-external-login-provider-test',
		loader: () => import('./external-login-provider-test.element'),
		weight: 2,
		meta: {
			label: 'Test External Login Provider',
			pathname: 'test/test/test',
		},
	},
	{
		type: 'externalLoginProvider',
		alias: 'Umb.ExternalLoginProvider.Test2',
		name: 'Test External Login Provider 2',
		elementName: 'umb-external-login-provider-test2',
		loader: () => import('./external-login-provider-test2.element'),
		weight: 1,
		meta: {
			label: 'Test External Login Provider 2',
			pathname: 'test/test/test',
		},
	},
];
