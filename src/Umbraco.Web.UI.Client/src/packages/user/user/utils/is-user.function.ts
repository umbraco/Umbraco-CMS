import { UmbUserDetailRepository } from '../repository/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export const isUserAdmin = async (host: UmbControllerHost, userUnique: string) => {
	const repository = new UmbUserDetailRepository(host);
	const { data: user } = await repository.requestByUnique(userUnique);

	//return user?.isAdmin;
	return false;
};
