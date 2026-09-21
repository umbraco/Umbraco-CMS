import { UmbSearchContext } from './search.global-context.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		// Kept on the spelling it shipped with in the standalone Umbraco.Cms.Search package.
		// Unlike the repository and store aliases, an extension that renders cannot be registered
		// under a second alias without appearing twice, so this one cannot be renamed and shimmed.
		// eslint-disable-next-line local-rules/enforce-manifest-alias
		alias: 'Umbraco.Search.GlobalContext',
		name: 'Umbraco Search Global Context',
		api: UmbSearchContext,
	},
];
