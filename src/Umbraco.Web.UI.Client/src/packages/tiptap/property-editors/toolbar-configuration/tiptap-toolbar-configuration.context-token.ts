import type { UmbTiptapToolbarConfigurationContext } from './tiptap-toolbar-configuration.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TIPTAP_TOOLBAR_CONFIGURATION_CONTEXT = new UmbContextToken<UmbTiptapToolbarConfigurationContext>(
	'UmbTiptapToolbarConfigurationContext',
);
