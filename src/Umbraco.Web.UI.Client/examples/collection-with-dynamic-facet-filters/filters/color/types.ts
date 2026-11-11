import type { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../entity.js';
import type { ExampleAttributeModel, ExampleFacetCount } from '../../data/types.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

export interface ExampleColorItemModel extends UmbItemModel {
	entityType: typeof EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE;
	name: string;
	hex: string;
}

export interface ExampleColorAttributeModel extends ExampleAttributeModel {
	hex: string;
}

export interface ExampleColorFacetCount extends ExampleFacetCount {
	hex: string;
}
