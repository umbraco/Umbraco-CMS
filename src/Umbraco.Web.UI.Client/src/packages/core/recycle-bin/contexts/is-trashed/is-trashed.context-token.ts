import type { UmbIsTrashedContext } from './is-trashed.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_IS_TRASHED_CONTEXT = new UmbContextToken<UmbIsTrashedContext>('UmbIsTrashedContext');
