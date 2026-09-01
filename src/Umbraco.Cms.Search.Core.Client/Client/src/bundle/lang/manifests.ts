import english from './en.js';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'localization',
    name: 'Umbraco Search Localization - English',
    alias: 'Umbraco.Search.Localization.En',
    meta: { culture: 'en', localizations: english },
  },
  {
    type: 'localization',
    name: 'Umbraco Search Localization - Danish',
    alias: 'Umbraco.Search.Localization.Da',
    meta: { culture: 'da' },
    js: () => import('./da.js'),
  },
];
