import type { UmbContentCollectionConfigurationContext } from './content-collection-configuration.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_CONTENT_COLLECTION_CONFIGURATION_CONTEXT =
	new UmbContextToken<UmbContentCollectionConfigurationContext>('UmbContentCollectionConfigurationContext');
