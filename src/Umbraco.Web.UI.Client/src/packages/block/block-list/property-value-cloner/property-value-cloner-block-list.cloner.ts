import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import {
	UmbFlatLayoutBlockPropertyValueCloner,
	type UmbBlockPropertyValueClonerArgs,
} from '@umbraco-cms/backoffice/block';

export class UmbBlockListPropertyValueCloner extends UmbFlatLayoutBlockPropertyValueCloner {
	constructor(args: UmbBlockPropertyValueClonerArgs) {
		super(UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS, args);
	}
}

export { UmbBlockListPropertyValueCloner as api };
