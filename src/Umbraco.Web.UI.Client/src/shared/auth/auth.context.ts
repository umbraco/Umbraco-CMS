import { IUmbAuth } from './auth.interface.js';
import { UmbAuthFlow } from './auth-flow.js';
import { UmbLoggedInUser } from './types.js';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { ReplaySubject } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbAuthContext implements IUmbAuth {
	#currentUser = new UmbObjectState<UmbLoggedInUser | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();
	readonly isLoggedIn = new ReplaySubject<boolean>(1);

	#host;
	#authFlow;

	constructor(host: UmbControllerHostElement, authFlow: UmbAuthFlow) {
		this.#host = host;
		this.#authFlow = authFlow;

		this.isLoggedIn.subscribe((isLoggedIn) => {
			if (isLoggedIn) {
				this.fetchCurrentUser();
			}
		});
	}

	async fetchCurrentUser(): Promise<UmbLoggedInUser | undefined> {
		const { data } = await tryExecuteAndNotify(this.#host, UserResource.getUserCurrent());

		if (!data) return;

		this.#currentUser.next(data);

		return data;
	}

	performWithFreshTokens(): Promise<string> {
		return this.#authFlow.performWithFreshTokens();
	}

	signOut(): Promise<void> {
		return this.#authFlow.signOut();
	}
}
