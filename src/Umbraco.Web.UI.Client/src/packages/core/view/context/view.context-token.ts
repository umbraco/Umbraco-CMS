import type { UmbViewContext } from './view.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VIEW_CONTEXT = new UmbContextToken<UmbViewContext>('UmbViewContext');
