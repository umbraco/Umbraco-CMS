import { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../entity.js';
import { productColors } from '../../../mock-data/product-colors.js';
import type { ExampleProductColorItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

/**
 * Resolves color uniques into color items, with the hex of each color.
 */
export class ExampleProductColorItemRepository
	extends UmbControllerBase
	implements UmbItemRepository<ExampleProductColorItemModel>
{
	async requestItems(uniques: Array<string>) {
		const data = productColors
			.filter((item) => uniques.includes(item.id))
			.map(
				(item): ExampleProductColorItemModel => ({
					entityType: EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					hex: item.hex,
				}),
			);
		return { data };
	}
}

export { ExampleProductColorItemRepository as api };
