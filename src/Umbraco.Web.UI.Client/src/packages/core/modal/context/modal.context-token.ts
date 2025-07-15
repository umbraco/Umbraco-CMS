import type { UmbModalContext } from './modal.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MODAL_CONTEXT = new UmbContextToken<UmbModalContext>('UmbModalContext');
