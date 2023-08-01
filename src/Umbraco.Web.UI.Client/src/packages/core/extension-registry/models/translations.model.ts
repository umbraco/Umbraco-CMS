import type { ManifestDefaultExport } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTranslations extends ManifestDefaultExport<Record<string, Record<string, string>>> {
	type: 'translations';
	meta: MetaTranslations;
}

export interface MetaTranslations {
	/**
	 * The culture of the translations.
	 * @example "en-US"
	 */
	culture: string;

	/**
	 * The direction of the translations (left-to-right or right-to-left).
	 * @example "ltr"
	 * @default "ltr"
	 */
	direction?: 'ltr' | 'rtl';

	/**
	 * The translations.
	 * @example
	 * {
	 *   "general": {
	 *     "cancel": "Cancel",
	 *     "close": "Close"
	 *   }
	 * }
	 */
	translations?: Record<string, Record<string, string>>;
}
