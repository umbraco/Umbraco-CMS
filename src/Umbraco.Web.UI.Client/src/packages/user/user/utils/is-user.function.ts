import { UmbUserDetailRepository } from '../repository/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Check if the user is an admin
 * @param host - The controller host
 * @param userUnique - The user unique identifier
 * @returns Promise resolving to true if user is admin
 */
export async function isUserAdmin(host: UmbControllerHost, userUnique: string): Promise<boolean> {
	const repository = new UmbUserDetailRepository(host);
	const { data: user } = await repository.requestByUnique(userUnique);

	return user?.isAdmin ?? false;
}
