import type { ExampleRangeValueModel } from './types.js';
import type {
	UmbFacetFilterValueDisplayModel,
	UmbFacetFilterValueResolver,
} from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of every range filter: `{ key: 'min', amount: 50 }` is displayed as "Min 50".
 * Registered on the range kind, so every filter using the kind gets it.
 */
export class ExampleRangeFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<ExampleRangeValueModel>
{
	async resolveValues(values: ReadonlyArray<ExampleRangeValueModel>) {
		return {
			data: values.map(
				(value): UmbFacetFilterValueDisplayModel => ({
					name: `${value.key === 'min' ? 'Min' : 'Max'} ${value.amount}`,
				}),
			),
		};
	}
}

export { ExampleRangeFacetFilterValueResolver as valueResolver };
