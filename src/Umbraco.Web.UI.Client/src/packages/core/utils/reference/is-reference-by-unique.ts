import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

/**
 *
 * @param {unknown} data The data to check if it is a ReferencedByUnique
 * @returns {boolean} True if the data is a ReferencedByUnique
 */
export function isReferenceByUnique(data: unknown): data is UmbReferenceByUnique {
	return (data as UmbReferenceByUnique).unique !== undefined;
}
