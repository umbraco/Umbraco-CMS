import type { UmbParentEntityContext } from './parent.entity-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PARENT_ENTITY_CONTEXT = new UmbContextToken<UmbParentEntityContext>('UmbParentEntityContext');
