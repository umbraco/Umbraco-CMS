import type { UmbAncestorsEntityContext } from './ancestors.entity-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ANCESTORS_ENTITY_CONTEXT = new UmbContextToken<UmbAncestorsEntityContext>('UmbAncestorsEntityContext');
