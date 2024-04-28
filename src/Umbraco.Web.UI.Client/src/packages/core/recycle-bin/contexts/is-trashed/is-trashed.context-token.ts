import type { IUmbIsTrashedContext } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_IS_TRASHED_CONTEXT = new UmbContextToken<IUmbIsTrashedContext>('UmbIsTrashedContext');
