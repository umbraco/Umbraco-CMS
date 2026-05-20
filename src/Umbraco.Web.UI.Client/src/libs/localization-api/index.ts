export * from './localization.controller.js';
export type * from './types/localization.js';
export * from './localization.manager.js';
// Side-effect re-export: registers the global `UmbKnownLocalizationSet` / `UmbKnownLocalizationKey`
// declarations so consumers (and plugin authors via interface merging) pick them up without an
// explicit type import. The runtime keys list is exposed for the staleness test in known-keys.test.ts.
export { UMB_KNOWN_LOCALIZATION_KEYS } from './known-keys.generated.js';
