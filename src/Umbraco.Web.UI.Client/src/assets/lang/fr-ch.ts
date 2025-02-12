/**
 * Creator Name: The Umbraco community
 * Creator Link: https://docs.umbraco.com/umbraco-cms/extending/language-files
 *
 * Language Alias: fr_ch
 * Language Int Name: French Switzerland (FR-CH)
 * Language Local Name: Français Suisse (FR-CH)
 * Language LCID: 12
 * Language Culture: fr-CH
 */
import fr_fr from './fr-fr.js';
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
	// NOTE: Imports and re-exports the French (France) localizations, so that any French (Switzerland) localizations can be override them. [LK]
	...fr_fr,
} as UmbLocalizationDictionary;
