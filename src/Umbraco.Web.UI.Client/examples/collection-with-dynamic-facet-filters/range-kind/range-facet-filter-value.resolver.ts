import { EXAMPLE_RANGE_LIMIT_ENTITY_TYPE } from './entity.js';
import type { ExampleRangeValueModel } from './types.js';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of every range filter into range limit items: `{ key: 'min', amount: 50 }` gets the
 * name "Min 50". Registered on the price filter's active value.
 */
export class ExampleRangeFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<ExampleRangeValueModel>
{
	async resolveValues(values: ReadonlyArray<ExampleRangeValueModel>) {
		return {
			data: values.map(
				(value): UmbItemModel => ({
					entityType: EXAMPLE_RANGE_LIMIT_ENTITY_TYPE,
					unique: value.key,
					name: `${value.key === 'min' ? 'Min' : 'Max'} ${value.amount}`,
				}),
			),
		};
	}
}

export { ExampleRangeFacetFilterValueResolver as valueResolver };
