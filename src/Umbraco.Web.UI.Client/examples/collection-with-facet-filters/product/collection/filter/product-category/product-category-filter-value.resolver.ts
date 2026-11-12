import { ExampleProductCategoryItemRepository } from '../../../../product-category/repository/item/product-category-item.repository.js';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the category filter into category items: `cat-tshirt` gets the name "T-shirts".
 * Registered on the category filter's active value.
 */
export class ExampleProductCategoryFilterValueResolver
	extends UmbControllerBase
	implements UmbFilterValueResolver<string>
{
	#itemRepository = new ExampleProductCategoryItemRepository(this);

	async resolveValues(values: ReadonlyArray<string>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values)]);
		return { data: values.map((value): UmbItemModel | undefined => data.find((item) => item.unique === value)) };
	}
}

export { ExampleProductCategoryFilterValueResolver as valueResolver };
