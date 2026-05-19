import type { UmbBlockGridValueModel } from '../../../types.js';

export const UMB_BLOCK_GRID_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.BlockGrid' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_BLOCK_GRID_PROPERTY_EDITOR_VALUE_TYPE]: UmbBlockGridValueModel;
	}
}
