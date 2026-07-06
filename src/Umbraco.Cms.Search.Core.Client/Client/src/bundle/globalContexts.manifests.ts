import { UmbSearchContext } from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'globalContext',
    alias: 'Umbraco.Search.GlobalContext',
    name: 'Umbraco Search Global Context',
    api: UmbSearchContext,
  },
];
