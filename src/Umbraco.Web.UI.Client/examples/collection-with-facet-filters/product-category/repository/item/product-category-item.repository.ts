import { EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE } from '../../entity.js';
import { productCategories } from '../../product-categories.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

/**
 * Resolves category uniques into category items.
 */
export class ExampleProductCategoryItemRepository extends UmbControllerBase implements UmbItemRepository<UmbItemModel> {
	async requestItems(uniques: Array<string>) {
		const data = productCategories
			.filter((item) => uniques.includes(item.id))
			.map(
				(item): UmbItemModel => ({
					entityType: EXAMPLE_PRODUCT_CATEGORY_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
				}),
			);
		return { data };
	}
}

export { ExampleProductCategoryItemRepository as api };
