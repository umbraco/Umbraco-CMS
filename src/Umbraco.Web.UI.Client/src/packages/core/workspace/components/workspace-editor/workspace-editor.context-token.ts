import type { UmbWorkspaceEditorContext } from './workspace-editor.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_WORKSPACE_EDITOR_CONTEXT = new UmbContextToken<UmbWorkspaceEditorContext>('UmbWorkspaceViewContext');
