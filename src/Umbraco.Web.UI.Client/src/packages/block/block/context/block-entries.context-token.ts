import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel } from '../types.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UmbBlockManagerContext } from './block-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_ENTRIES_CONTEXT = new UmbContextToken<
	typeof UmbBlockEntriesContext<
		UmbContextToken<UmbBlockManagerContext, UmbBlockManagerContext>,
		UmbBlockManagerContext<UmbBlockTypeBaseModel, UmbBlockLayoutBaseModel>,
		UmbBlockTypeBaseModel,
		UmbBlockLayoutBaseModel
	>
>('UmbBlockEntriesContext');
