import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Check if the current user is the user with the given unique id
 * @param host
 * @param userUnique
 */
export const isCurrentUser = async (host: UmbControllerHost, userUnique: string) => {
	const ctrl = new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT);
	const currentUserContext = await ctrl.asPromise();
	ctrl.destroy();

	return await currentUserContext!.isUserCurrentUser(userUnique);
};
