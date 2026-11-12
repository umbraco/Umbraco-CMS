import { ExampleProductColorItemRepository } from '../repository/item/product-color-item.repository.js';
import type { ExampleProductColorItemModel } from '../repository/item/types.js';
import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves lists of product color uniques into product color items, for every cell of a column in one request.
 */
export class ExampleProductColorValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string>, ReadonlyArray<ExampleProductColorItemModel>>
{
	#itemRepository = new ExampleProductColorItemRepository(this);

	async resolveValues(values: ReadonlyArray<Array<string>>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values.flat())]);
		const itemByUnique = new Map(data.map((item) => [item.unique, item]));
		return {
			data: values.map((uniques) =>
				uniques
					.map((unique) => itemByUnique.get(unique))
					.filter((item): item is ExampleProductColorItemModel => !!item)
					.sort((a, b) => a.name.localeCompare(b.name)),
			),
		};
	}
}

export { ExampleProductColorValueSummaryResolver as valueResolver };
