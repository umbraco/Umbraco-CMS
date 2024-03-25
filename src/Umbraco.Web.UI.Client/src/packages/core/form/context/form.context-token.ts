import type { UmbFormContext } from './form.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FORM_CONTEXT = new UmbContextToken<UmbFormContext>('UmbFormContext');
