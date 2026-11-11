import { EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE } from '../../entity.js';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

const categoryNames: Record<string, string> = {
	'cat-hoodie': 'Hoodies',
	'cat-jacket': 'Jackets',
	'cat-shorts': 'Shorts',
	'cat-trousers': 'Trousers',
	'cat-tshirt': 'T-shirts',
};

/**
 * Resolves the active values of the category filter into category items: `cat-tshirt` gets the name "T-shirts".
 * Registered on the category filter's active value.
 */
export class ExampleCategoryFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<string>
{
	async resolveValues(values: ReadonlyArray<string>) {
		return {
			data: values.map(
				(unique): UmbItemModel => ({
					entityType: EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE,
					unique,
					name: categoryNames[unique] ?? unique,
				}),
			),
		};
	}
}

export { ExampleCategoryFacetFilterValueResolver as valueResolver };
