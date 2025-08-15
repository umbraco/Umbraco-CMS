import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbHintController } from './hints.controller.js';

export const UMB_HINT_CONTEXT = new UmbContextToken<UmbHintController>('UmbHintContext');
