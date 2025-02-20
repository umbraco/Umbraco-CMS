import type { UmbBlockGridAreaConfigEntryContext } from './block-grid-area-config-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_GRID_AREA_CONFIG_ENTRY_CONTEXT = new UmbContextToken<UmbBlockGridAreaConfigEntryContext>(
	'UmbBlockAreaConfigEntryContext',
);
