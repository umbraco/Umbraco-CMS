import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Alias values carried over from the standalone `Umbraco.Cms.Search` package, where they predated
 * core's `{Domain}.{ExtensionType}.{Subject}` convention. The constants keep their names, so code
 * importing them is unaffected; these only exist for consumers that hard-coded the raw strings.
 *
 * Each is registered a second time alongside its replacement, so an extension still referencing the
 * old string resolves. Only aliases that are looked up are shimmed - re-registering an extension
 * that renders would show it twice, and re-registering the store would produce a second instance
 * competing for the same context token.
 */
export const UMB_SEARCH_LEGACY_COLLECTION_REPOSITORY_ALIAS = 'UMB_SEARCH_COLLECTION_REPOSITORY';
export const UMB_SEARCH_LEGACY_DETAIL_REPOSITORY_ALIAS = 'UmbSearchDetailRepository';
export const UMB_SEARCH_LEGACY_QUERY_REPOSITORY_ALIAS = 'UmbSearchQueryRepository';
export const UMB_SEARCH_LEGACY_ROOT_COLLECTION_ALIAS = 'UMB_SEARCH_ROOT_COLLECTION';

/**
 * Warns that a legacy alias was resolved, then hands back the module the replacement alias uses.
 * @param {string} deprecated The legacy alias that was resolved.
 * @param {string} replacement The alias that should be referenced instead.
 * @template T The module the replacement alias loads.
 * @param {() => Promise<T>} load Loader for the implementation the replacement alias registers.
 * @returns {Promise<T>} The loaded module.
 */
export async function loadWithDeprecationWarning<T>(
	deprecated: string,
	replacement: string,
	load: () => Promise<T>,
): Promise<T> {
	new UmbDeprecation({
		deprecated: `The extension alias "${deprecated}" is deprecated.`,
		removeInVersion: '21.0.0',
		solution: `Reference "${replacement}" instead.`,
	}).warn({ logAlways: true });

	return load();
}
