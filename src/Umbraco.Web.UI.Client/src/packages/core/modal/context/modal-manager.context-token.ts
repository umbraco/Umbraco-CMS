import type { UmbModalManagerContext } from './modal-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MODAL_MANAGER_CONTEXT = new UmbContextToken<UmbModalManagerContext, UmbModalManagerContext>(
	'UmbModalManagerContext',
);
