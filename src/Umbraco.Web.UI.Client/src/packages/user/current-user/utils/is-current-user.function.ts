import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export const isCurrentUser = async (host: UmbControllerHost, userUnique: string) => {
	const ctrl = new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT);
	let currentUserContext = await ctrl.asPromise();
	ctrl.destroy();

	const controller = new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT, (context) => {
		currentUserContext = context;
	});

	await controller.asPromise();

	controller.destroy();

	return await currentUserContext!.isUserCurrentUser(userUnique);
};
