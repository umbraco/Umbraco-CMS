import type { UmbBlockGridAreaTypeEntriesContext } from './block-grid-area-type-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_GRID_AREA_TYPE_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockGridAreaTypeEntriesContext>(
	'UmbBlockGridAreaTypeEntriesContext',
);
