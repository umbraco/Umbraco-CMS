import english from './en.js';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'localization',
    name: 'Umbraco Search Examine Localization - English',
    alias: 'Umbraco.Search.Examine.Localization.En',
    meta: { culture: 'en', localizations: english },
  },
  {
    type: 'localization',
    name: 'Umbraco Search Examine Localization - Danish',
    alias: 'Umbraco.Search.Examine.Localization.Da',
    meta: { culture: 'da' },
    js: () => import('./da.js'),
  },
];
