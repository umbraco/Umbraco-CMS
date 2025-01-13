import type { UmbPropertyClipboardContext } from './property-clipboard.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_CLIPBOARD_CONTEXT = new UmbContextToken<UmbPropertyClipboardContext>(
	'UmbPropertyClipboardContext',
);
