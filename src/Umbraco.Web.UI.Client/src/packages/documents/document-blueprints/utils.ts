import { sortVariants as _sortVariants } from '@umbraco-cms/backoffice/variant';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbDocumentBlueprintVariantOptionModel } from './types.js';

type VariantType = UmbDocumentBlueprintVariantOptionModel;

/**
 * Sorts document blueprint variants based on multiple criteria:
 * @param {VariantType} a - First variant to compare
 * @param {VariantType} b - Second variant to compare
 * @returns {number} - Sorting value
 * @deprecated Deprecated since v17.6. Import `sortVariants` from `@umbraco-cms/backoffice/variant` instead. Scheduled for removal in Umbraco 19.
 */
export function sortVariants(a: VariantType, b: VariantType): number {
	new UmbDeprecation({
		deprecated: 'sortVariants from @umbraco-cms/backoffice/document-blueprint',
		removeInVersion: '19.0.0',
		solution: 'Import `sortVariants` from `@umbraco-cms/backoffice/variant` instead.',
	}).warn();

	return _sortVariants(a, b);
}
