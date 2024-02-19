import type { UmbBlockManagerContext } from './block-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_MANAGER_CONTEXT = new UmbContextToken<UmbBlockManagerContext>('UmbBlockManagerContext');
