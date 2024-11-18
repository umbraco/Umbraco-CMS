import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbClipboardContext } from './clipboard.context.js';

export const UMB_CLIPBOARD_CONTEXT = new UmbContextToken<UmbClipboardContext>('UmbClipboardContext');
