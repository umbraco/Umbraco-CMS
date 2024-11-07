/**
 * Creator Name: The Umbraco community
 * Creator Link: https://docs.umbraco.com/umbraco-cms/extending/language-files
 *
 * Language Alias: it_ch
 * Language Int Name: Italian Switzerland (IT-CH)
 * Language Local Name: Italiano Svizerra (IT-CH)
 * Language LCID: 16
 * Language Culture: it-CH
 */
import it_it from './it-it.js';
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
	// NOTE: Imports and re-exports the Italian (Italy) localizations, so that any Italian (Switzerland) localizations can be override them. [LK]
	...it_it,
} as UmbLocalizationDictionary;
