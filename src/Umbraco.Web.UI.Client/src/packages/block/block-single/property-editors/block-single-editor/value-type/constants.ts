import type { UmbBlockSingleValueModel } from '../../../types.js';

export const UMB_BLOCK_SINGLE_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.SingleBlock' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_VALUE_TYPE]: UmbBlockSingleValueModel;
	}
}
