import type { UmbHintController } from './hint.controller.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_HINT_CONTEXT = new UmbContextToken<UmbHintController>('UmbHintContext');
