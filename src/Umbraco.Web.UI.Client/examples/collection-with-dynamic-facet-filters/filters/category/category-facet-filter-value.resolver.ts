import { EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE } from '../../entity.js';
import { categories } from './categories.js';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

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
			data: values.map((value): UmbItemModel | undefined => {
				const category = categories.find((item) => item.id === value);
				if (!category) return undefined;
				return { entityType: EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE, unique: category.id, name: category.name };
			}),
		};
	}
}

export { ExampleCategoryFacetFilterValueResolver as valueResolver };
