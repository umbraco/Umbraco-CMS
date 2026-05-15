import type { UmbBlockListValueModel } from '../../../types.js';

export const UMB_BLOCK_LIST_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.BlockList' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_BLOCK_LIST_PROPERTY_EDITOR_VALUE_TYPE]: UmbBlockListValueModel | undefined;
	}
}
