import type { UmbContentPropertyContext } from './content-property.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CONTENT_PROPERTY_CONTEXT = new UmbContextToken<UmbContentPropertyContext>('UmbContentPropertyContext');
