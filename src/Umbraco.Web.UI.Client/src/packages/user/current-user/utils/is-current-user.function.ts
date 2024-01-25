import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export const isCurrentUser = async (host: UmbControllerHost, userId: string) => {
	let currentUserContext: typeof UMB_CURRENT_USER_CONTEXT.TYPE | undefined;

	await new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT, (context) => {
		currentUserContext = context;
	}).asPromise();

	return await currentUserContext!.isUserCurrentUser(userId);
};
