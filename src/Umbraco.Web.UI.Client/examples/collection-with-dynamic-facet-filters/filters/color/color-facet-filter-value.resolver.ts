import { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../entity.js';
import type { ExampleColorItemModel } from './types.js';
import { exampleColors } from './colors.js';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the color filter into color items: `color-white` gets the name "White" and its hex.
 * Registered on the color filter's active value.
 */
export class ExampleColorFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<string, ExampleColorItemModel>
{
	async resolveValues(values: ReadonlyArray<string>) {
		return {
			data: values.map(
				(unique): ExampleColorItemModel => ({
					entityType: EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE,
					unique,
					name: exampleColors[unique]?.name ?? unique,
					hex: exampleColors[unique]?.hex ?? unique,
				}),
			),
		};
	}
}

export { ExampleColorFacetFilterValueResolver as valueResolver };
