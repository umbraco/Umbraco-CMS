import type { UmbSearchWorkspaceContext } from './search-workspace.context.js';
import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '@umbraco-cms/search/global';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_SEARCH_WORKSPACE_CONTEXT = new UmbContextToken<
  UmbWorkspaceContext,
  UmbSearchWorkspaceContext
>(
  'UmbWorkspaceContext',
  undefined,
  (context): context is UmbSearchWorkspaceContext =>
    context.getEntityType?.() === UMB_SEARCH_INDEX_ENTITY_TYPE,
);
