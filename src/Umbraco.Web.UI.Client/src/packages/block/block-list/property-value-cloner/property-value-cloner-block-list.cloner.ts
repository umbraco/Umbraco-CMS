import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { UmbFlatLayoutBlockPropertyValueCloner } from '@umbraco-cms/backoffice/block';

export class UmbBlockListPropertyValueCloner extends UmbFlatLayoutBlockPropertyValueCloner {
	constructor() {
		super(UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS);
	}
}
