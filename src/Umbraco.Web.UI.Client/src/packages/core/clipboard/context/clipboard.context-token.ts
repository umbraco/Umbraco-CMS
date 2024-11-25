import type { UmbClipboardContext } from './clipboard.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CLIPBOARD_CONTEXT = new UmbContextToken<UmbClipboardContext>('UmbClipboardContext');
