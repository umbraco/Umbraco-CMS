import { EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE } from '../../entity.js';
import { productSizes } from '../../product-sizes.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

/**
 * Resolves size uniques into size items.
 */
export class ExampleProductSizeItemRepository extends UmbControllerBase implements UmbItemRepository<UmbItemModel> {
	async requestItems(uniques: Array<string>) {
		const data = productSizes
			.filter((item) => uniques.includes(item.id))
			.map(
				(item): UmbItemModel => ({ entityType: EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE, unique: item.id, name: item.name }),
			);
		return { data };
	}
}

export { ExampleProductSizeItemRepository as api };
