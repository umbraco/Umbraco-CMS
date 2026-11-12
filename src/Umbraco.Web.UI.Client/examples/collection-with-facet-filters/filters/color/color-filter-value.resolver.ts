import { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../entity.js';
import { colors } from './colors.js';
import type { ExampleColorItemModel } from './types.js';
import type { UmbFilterValueResolver } from '@umbraco-cms/backoffice/filter';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Resolves the active values of the color filter into color items: `color-white` gets the name "White" and its hex.
 * Registered on the color filter's active value.
 */
export class ExampleColorFilterValueResolver
	extends UmbControllerBase
	implements UmbFilterValueResolver<string, ExampleColorItemModel>
{
	async resolveValues(values: ReadonlyArray<string>) {
		return {
			data: values.map((value): ExampleColorItemModel | undefined => {
				const color = colors.find((item) => item.id === value);
				if (!color) return undefined;
				return { entityType: EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE, unique: color.id, name: color.name, hex: color.hex };
			}),
		};
	}
}

export { ExampleColorFilterValueResolver as valueResolver };
