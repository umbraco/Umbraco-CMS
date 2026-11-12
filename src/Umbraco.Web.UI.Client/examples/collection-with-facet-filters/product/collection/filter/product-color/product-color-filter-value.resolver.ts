import { ExampleProductColorItemRepository } from '../../../../product-color/repository/item/product-color-item.repository.js';
import type { ExampleProductColorItemModel } from '../../../../product-color/repository/item/types.js';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the color filter into color items: `color-white` gets the name "White" and its hex.
 * Registered on the color filter's active value.
 */
export class ExampleProductColorFilterValueResolver
	extends UmbControllerBase
	implements UmbFilterValueResolver<string, ExampleProductColorItemModel>
{
	#itemRepository = new ExampleProductColorItemRepository(this);

	async resolveValues(values: ReadonlyArray<string>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values)]);
		return {
			data: values.map((value): ExampleProductColorItemModel | undefined => data.find((item) => item.unique === value)),
		};
	}
}

export { ExampleProductColorFilterValueResolver as valueResolver };
