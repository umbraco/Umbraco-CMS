/**
 * Creator Name: The Umbraco community
 * Creator Link: https://docs.umbraco.com/umbraco-cms/extending/language-files
 *
 * Language Alias: de_CH
 * Language Int Name: German Switzerland (DE-CH)
 * Language Local Name: Deutsch Schweiz (DE-CH)
 * Language LCID: 7
 * Language Culture: de-CH
 */
import de_de from './de-de.js';
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
	// NOTE: Imports and re-exports the German (Germany) localizations, so that any German (Switzerland) localizations can be override them. [LK]
	...de_de,
} as UmbLocalizationDictionary;
