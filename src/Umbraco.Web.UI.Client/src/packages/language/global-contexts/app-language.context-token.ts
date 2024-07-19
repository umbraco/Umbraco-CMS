import type { UmbAppLanguageContext } from './app-language.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_APP_LANGUAGE_CONTEXT = new UmbContextToken<UmbAppLanguageContext>('UmbAppLanguageContext');
