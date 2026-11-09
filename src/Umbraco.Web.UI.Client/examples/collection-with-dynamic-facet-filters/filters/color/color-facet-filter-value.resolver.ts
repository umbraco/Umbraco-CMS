import { exampleColors } from './colors.js';
import type { UmbFacetFilterValueResolver } from '@umbraco-cms/backoffice/facet-filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the color filter: `color-white` is displayed as "White".
 * Registered on the color filter only.
 */
export class ExampleColorFacetFilterValueResolver
	extends UmbControllerBase
	implements UmbFacetFilterValueResolver<string>
{
	async resolveValues(values: ReadonlyArray<string>) {
		return { data: values.map((value) => ({ name: exampleColors[value]?.name ?? value })) };
	}
}

export { ExampleColorFacetFilterValueResolver as valueResolver };
