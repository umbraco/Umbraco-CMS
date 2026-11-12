import { ExampleProductSizeItemRepository } from '../../../../product-size/repository/item/product-size-item.repository.js';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the size filter into size items: `size-xl` gets the name "XL".
 * Registered on the size filter's active value.
 */
export class ExampleProductSizeFilterValueResolver extends UmbControllerBase implements UmbFilterValueResolver<string> {
	#itemRepository = new ExampleProductSizeItemRepository(this);

	async resolveValues(values: ReadonlyArray<string>) {
		const { data = [] } = await this.#itemRepository.requestItems([...new Set(values)]);
		return { data: values.map((value): UmbItemModel | undefined => data.find((item) => item.unique === value)) };
	}
}

export { ExampleProductSizeFilterValueResolver as valueResolver };
