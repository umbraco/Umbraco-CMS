import { ExampleProductCategoryItemRepository } from '../repository/item/product-category-item.repository.js';
import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves product category uniques into product category items, for every cell of a column in one request.
 */
export class ExampleProductCategoryValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<string, UmbItemModel | undefined>
{
	#itemRepository = new ExampleProductCategoryItemRepository(this);

	async resolveValues(values: ReadonlyArray<string>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values)]);
		return { data: values.map((value) => data.find((item) => item.unique === value)) };
	}
}

export { ExampleProductCategoryValueSummaryResolver as valueResolver };
