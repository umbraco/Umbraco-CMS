import type { UmbPreviewContext } from './preview.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PREVIEW_CONTEXT = new UmbContextToken<UmbPreviewContext>('UmbPreviewContext');
