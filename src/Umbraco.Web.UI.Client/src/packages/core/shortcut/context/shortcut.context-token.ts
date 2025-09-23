import type { UmbShortcutController } from './shortcut.controller.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_SHORTCUT_CONTEXT = new UmbContextToken<UmbShortcutController>('UmbShortcutContext');
