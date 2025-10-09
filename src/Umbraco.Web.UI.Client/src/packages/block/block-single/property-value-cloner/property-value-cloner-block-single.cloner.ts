import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import {
	UmbFlatLayoutBlockPropertyValueCloner,
	type UmbBlockPropertyValueClonerArgs,
} from '@umbraco-cms/backoffice/block';

export class UmbBlockSinglePropertyValueCloner extends UmbFlatLayoutBlockPropertyValueCloner {
	constructor(args: UmbBlockPropertyValueClonerArgs) {
		super(UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS, args);
	}
}

export { UmbBlockSinglePropertyValueCloner as api };
