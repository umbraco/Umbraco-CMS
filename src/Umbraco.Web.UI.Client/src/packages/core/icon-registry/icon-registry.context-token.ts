import type { UmbIconRegistryContext } from './icon-registry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ICON_REGISTRY_CONTEXT = new UmbContextToken<UmbIconRegistryContext>('UmbIconRegistryContext');
