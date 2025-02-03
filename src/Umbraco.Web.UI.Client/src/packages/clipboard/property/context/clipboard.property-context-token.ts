import type { UmbClipboardPropertyContext } from './clipboard.property-context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CLIPBOARD_PROPERTY_CONTEXT = new UmbContextToken<UmbClipboardPropertyContext>(
	'UmbPropertyContext',
	'UmbClipboardPropertyContext',
);
