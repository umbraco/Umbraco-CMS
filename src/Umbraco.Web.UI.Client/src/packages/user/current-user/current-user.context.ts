import { UmbCurrentUser } from './types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UserResource } from '@umbraco-cms/backoffice/backend-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbLocalizationRegistry } from '@umbraco-cms/backoffice/localization';

export class UmbCurrentUserContext extends UmbBaseController {
	#currentUser = new UmbObjectState<UmbCurrentUser | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();

	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode ?? 'en-us');

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_AUTH_CONTEXT, (instance) => {
			this.#authContext = instance;
			this.#observeIsAuthorized();
		});

		// TODO: revisit this. It can probably be simplified
		this.observe(umbLocalizationRegistry.isDefaultLoaded, (isDefaultLoaded) => {
			if (!isDefaultLoaded) return;

			this.observe(
				this.languageIsoCode,
				(currentLanguageIsoCode) => {
					umbLocalizationRegistry.loadLanguage(currentLanguageIsoCode);
				},
				'umbCurrentUserLanguageIsoCode',
			);
		});

		this.provideContext(UMB_CURRENT_USER_CONTEXT, this);
	}

	async requestCurrentUser() {
		// TODO: use repository
		const { data, error } = await tryExecuteAndNotify(this._host, UserResource.getUserCurrent());
		// TODO: add current user store
		this.#currentUser.setValue(data);
		return { data, error };
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

	#observeIsAuthorized() {
		if (!this.#authContext) return;
		this.observe(this.#authContext.isAuthorized, (isAuthorized) => {
			if (isAuthorized) {
				this.requestCurrentUser();
			}
		});
	}
}

export const UMB_CURRENT_USER_CONTEXT = new UmbContextToken<UmbCurrentUserContext>('UmbCurrentUserContext');
