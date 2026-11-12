import { ExampleProductSizeItemRepository } from '../repository/item/product-size-item.repository.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbValueSummaryResolver } from '@umbraco-cms/backoffice/value-summary';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves lists of product size uniques into product size items, for every cell of a column in one request.
 */
export class ExampleProductSizeValueSummaryResolver
	extends UmbControllerBase
	implements UmbValueSummaryResolver<Array<string>, ReadonlyArray<UmbItemModel>>
{
	#itemRepository = new ExampleProductSizeItemRepository(this);

	async resolveValues(values: ReadonlyArray<Array<string>>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values.flat())]);
		const itemByUnique = new Map(data.map((item) => [item.unique, item]));
		return {
			data: values.map((uniques) =>
				uniques.map((unique) => itemByUnique.get(unique)).filter((item): item is UmbItemModel => !!item),
			),
		};
	}
}

export { ExampleProductSizeValueSummaryResolver as valueResolver };
