import { EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE } from '../../entity.js';
import { sizes } from './sizes.js';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the size filter into size items: `size-xl` gets the name "XL".
 * Registered on the size filter's active value.
 */
export class ExampleSizeFilterValueResolver extends UmbControllerBase implements UmbFilterValueResolver<string> {
	async resolveValues(values: ReadonlyArray<string>) {
		return {
			data: values.map((value): UmbItemModel | undefined => {
				const size = sizes.find((item) => item.id === value);
				if (!size) return undefined;
				return { entityType: EXAMPLE_PRODUCT_SIZE_ENTITY_TYPE, unique: size.id, name: size.name };
			}),
		};
	}
}

export { ExampleSizeFilterValueResolver as valueResolver };
