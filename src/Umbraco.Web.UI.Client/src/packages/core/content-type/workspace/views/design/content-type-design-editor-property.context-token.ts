import type { UmbPropertyTypeContext } from './content-type-design-editor-property.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_TYPE_CONTEXT = new UmbContextToken<UmbPropertyTypeContext>('UmbPropertyTypeContext');
