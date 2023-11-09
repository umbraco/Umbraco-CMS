import { UmbAuth, UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export const isCurrentUser = async (host: UmbControllerHost, userId: string) => {
	let authContext: UmbAuth | undefined = undefined;

	await new UmbContextConsumerController(host, UMB_AUTH_CONTEXT, (context) => {
		authContext = context;
	}).asPromise();

	return await authContext!.isUserCurrentUser(userId);
};
