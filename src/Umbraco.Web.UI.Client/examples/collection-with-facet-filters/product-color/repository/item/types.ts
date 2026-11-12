import type { EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE } from '../../entity.js';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

export interface ExampleProductColorItemModel extends UmbItemModel {
	entityType: typeof EXAMPLE_PRODUCT_COLOR_ENTITY_TYPE;
	name: string;
	hex: string;
}
