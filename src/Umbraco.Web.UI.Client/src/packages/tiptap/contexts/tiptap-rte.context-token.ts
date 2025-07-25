import type { UmbTiptapRteContext } from './tiptap-rte.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TIPTAP_RTE_CONTEXT = new UmbContextToken<UmbTiptapRteContext>('UmbTiptapRteContext');
