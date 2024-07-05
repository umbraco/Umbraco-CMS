import type UmbCurrentUserContext from './current-user.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CURRENT_USER_CONTEXT = new UmbContextToken<UmbCurrentUserContext>('UmbCurrentUserContext');
