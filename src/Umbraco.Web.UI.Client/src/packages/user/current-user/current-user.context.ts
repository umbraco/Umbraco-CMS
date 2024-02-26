import type { UmbCurrentUserModel } from './types.js';
import { UmbCurrentUserRepository } from './repository/index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbLocalizationRegistry } from '@umbraco-cms/backoffice/localization';

export class UmbCurrentUserContext extends UmbBaseController {
	#currentUser = new UmbObjectState<UmbCurrentUserModel | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable();

	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode);

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_AUTH_CONTEXT, (instance) => {
			this.#authContext = instance;
			this.#observeIsAuthorized();
		});

		this.observe(this.languageIsoCode, (currentLanguageIsoCode) => {
			if (!currentLanguageIsoCode) return;
			umbLocalizationRegistry.loadLanguage(currentLanguageIsoCode);
		});

		this.provideContext(UMB_CURRENT_USER_CONTEXT, this);
	}

	async requestCurrentUser() {
		const { data } = await this.#currentUserRepository.requestCurrentUser();

		if (data) {
			// TODO: observe current user
			this.#currentUser.setValue(data);
		}
	}

	/**
	 * Checks if a user is the current user.
	 *
	 * @param userUnique The user id to check
	 * @returns True if the user is the current user, otherwise false
	 */
	async isUserCurrentUser(userUnique: string): Promise<boolean> {
		const currentUser = await firstValueFrom(this.currentUser);
		return currentUser?.unique === userUnique;
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

export default UmbCurrentUserContext;

export const UMB_CURRENT_USER_CONTEXT = new UmbContextToken<UmbCurrentUserContext>('UmbCurrentUserContext');
