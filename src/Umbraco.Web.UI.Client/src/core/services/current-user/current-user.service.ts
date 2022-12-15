import { BehaviorSubject, Observable } from 'rxjs';
import { umbUsersData } from '../../mocks/data/users.data';
import type { UserDetails } from '@umbraco-cms/models';
import { umbracoPath } from '@umbraco-cms/utils';

class UmbCurrentUserService {
	private _currentUser = new BehaviorSubject<UserDetails>(umbUsersData.getAll()[0]); //TODO: Temp solution to set the first user as the current logged in user
	public readonly currentUser: Observable<UserDetails> = this._currentUser.asObservable();

	/**
	 * logs out the user
	 * @public
	 * @memberof UmbCurrentUserService
	 */
	public logout(): void {
		fetch(umbracoPath('/user/logout').toString())
			.then((res) => res.json())
			.then((data) => {
				console.log('User Logged out', data);
			});
	}

	public get isAdmin(): boolean {
		//TODO: Find a way to figure out if current user is in the admin group
		const adminUserGroupKey = 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1';
		return this._currentUser.getValue()?.userGroups.includes(adminUserGroupKey);
	}
}

export const umbCurrentUserService = new UmbCurrentUserService();
