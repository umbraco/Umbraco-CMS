import type { UmbEntityContext } from './entity.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_CONTEXT = new UmbContextToken<UmbEntityContext>('UmbEntityContext');
