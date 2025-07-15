import { UmbUserDetailRepository } from '../repository/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Check if the user is an admin
 * @param {UmbControllerHost} host - The controller host
 * @param {string} userUnique - The user unique identifier
 * @returns {Promise<boolean>} - The result
 */
export const isUserAdmin = async (host: UmbControllerHost, userUnique: string) => {
	const repository = new UmbUserDetailRepository(host);
	const { data: user } = await repository.requestByUnique(userUnique);

	return user?.isAdmin ?? false;
};
