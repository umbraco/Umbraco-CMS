import type { UmbReferenceByAlias } from '@umbraco-cms/backoffice/models';

/**
 *
 * @param {unknown} data The data to check if it is a ReferencedByUnique
 * @returns {boolean} True if the data is a ReferencedByUnique
 */
export function isReferenceByAlias(data: unknown): data is UmbReferenceByAlias {
	return (data as UmbReferenceByAlias).alias !== undefined;
}
