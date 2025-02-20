import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestGlobalContext = {
	type: 'globalContext',
	alias: 'Umb.GlobalContext.Ufm',
	name: 'UFM Context',
	api: () => import('./ufm.context.js'),
};
