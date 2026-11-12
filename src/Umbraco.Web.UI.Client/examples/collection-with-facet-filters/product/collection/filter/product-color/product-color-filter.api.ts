import type { ExampleProductColorItemModel } from '../../../../product-color/repository/item/types.js';
import { UmbFacetSelectFilterApi } from '@umbraco-cms/backoffice/filter';
import type { UmbFacetModel, UmbSelectFilterOptionModel } from '@umbraco-cms/backoffice/filter';

export interface ExampleProductColorFacetOptionModel extends UmbSelectFilterOptionModel {
	name: string;
	hex: string;
}

/**
 * Adds the hex of each color to the options, for the swatches.
 */
export class ExampleProductColorFilterApi extends UmbFacetSelectFilterApi<
	ExampleProductColorFacetOptionModel,
	ExampleProductColorItemModel
> {
	protected override _toOption(facet: UmbFacetModel, item: ExampleProductColorItemModel | undefined) {
		return { ...facet, name: item?.name ?? facet.unique, hex: item?.hex ?? '' };
	}
}

export { ExampleProductColorFilterApi as api };
