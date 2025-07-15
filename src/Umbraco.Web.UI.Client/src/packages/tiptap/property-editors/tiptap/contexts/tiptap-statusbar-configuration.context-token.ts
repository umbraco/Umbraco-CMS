import type { UmbTiptapStatusbarConfigurationContext } from './tiptap-statusbar-configuration.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_TIPTAP_STATUSBAR_CONFIGURATION_CONTEXT = new UmbContextToken<UmbTiptapStatusbarConfigurationContext>(
	'UmbTiptapStatusbarConfigurationContext',
);
