import { UmbSearchContext } from './search.global-context.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Search',
		name: 'Umbraco Search Global Context',
		api: UmbSearchContext,
	},
];
