import { umbracoPath } from '@umbraco-cms/utils';
import type { UserDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ObjectState } from '@umbraco-cms/observable-api';

export class UmbCurrentUserStore {

	private _currentUser = new ObjectState<UserDetails | undefined>(undefined);
	public readonly currentUser = this._currentUser.asObservable();

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
		const currentUser = this._currentUser.getValue();
		return currentUser ? currentUser.userGroups.includes(adminUserGroupKey) : false;
	}
}

export const UMB_CURRENT_USER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbCurrentUserStore>(UmbCurrentUserStore.name);
