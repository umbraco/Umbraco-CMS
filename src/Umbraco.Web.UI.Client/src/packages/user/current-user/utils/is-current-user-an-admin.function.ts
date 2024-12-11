import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';

/**
 * Check if the current user is an admin
 * @param host
 */
export const isCurrentUserAnAdmin = async (host: UmbControllerHost) => {
	const ctrl = new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT);
	const currentUserContext = await ctrl.asPromise();
	ctrl.destroy();

	return currentUserContext!.isCurrentUserAdmin();
};
