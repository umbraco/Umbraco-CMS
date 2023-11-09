import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbLoggedInUser } from '@umbraco-cms/backoffice/auth';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbCurrentUserContext extends UmbBaseController {
	#currentUser = new UmbObjectState<UmbLoggedInUser | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();

	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode ?? 'en-us');

	constructor(host: UmbControllerHost) {
		super(host);
		this.provideContext(UMB_CURRENT_USER_CONTEXT, this);

		/*
		this.observe(this.isLoggedIn, (isLoggedIn) => {
			if (isLoggedIn) {
				this.fetchCurrentUser();
			}
		});
    */
	}

	/**
	 * Checks if a user is the current user.
	 *
	 * @param userId The user id to check
	 * @returns True if the user is the current user, otherwise false
	 */
	async isUserCurrentUser(userId: string): Promise<boolean> {
		const currentUser = await firstValueFrom(this.currentUser);
		return currentUser?.id === userId;
	}

	async fetchCurrentUser(): Promise<UmbLoggedInUser | undefined> {
		const { data } = await tryExecuteAndNotify(this._host, UserResource.getUserCurrent());

		this.#currentUser.next(data);

		return data;
	}
}

export const UMB_CURRENT_USER_CONTEXT = new UmbContextToken<UmbCurrentUserContext>('UmbCurrentUserContext');
