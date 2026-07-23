import type { UmbCurrentUserModel } from './types.js';
import { UmbCurrentUserRepository } from './repository/current-user.repository.js';
import { UMB_CURRENT_USER_CONTEXT } from './current-user.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbLocalizationRegistry } from '@umbraco-cms/backoffice/localization';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityDeletedEvent, UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_USER_ENTITY_TYPE } from '@umbraco-cms/backoffice/user';
import { UMB_USER_GROUP_ENTITY_TYPE } from '@umbraco-cms/backoffice/user-group';

export class UmbCurrentUserContext extends UmbContextBase {
	#currentUser = new UmbObjectState<UmbCurrentUserModel | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable().pipe(filter((user) => !!user));
	readonly allowedSections = this.#currentUser.asObservablePart((user) => user?.allowedSections);
	readonly avatarUrls = this.#currentUser.asObservablePart((user) => user?.avatarUrls);
	readonly documentStartNodeUniques = this.#currentUser.asObservablePart((user) => user?.documentStartNodeUniques);
	readonly elementStartNodeUniques = this.#currentUser.asObservablePart((user) => user?.elementStartNodeUniques);
	readonly email = this.#currentUser.asObservablePart((user) => user?.email);
	readonly fallbackPermissions = this.#currentUser.asObservablePart((user) => user?.fallbackPermissions);
	readonly hasAccessToAllLanguages = this.#currentUser.asObservablePart((user) => user?.hasAccessToAllLanguages);
	readonly hasAccessToSensitiveData = this.#currentUser.asObservablePart((user) => user?.hasAccessToSensitiveData);
	readonly hasDocumentRootAccess = this.#currentUser.asObservablePart((user) => user?.hasDocumentRootAccess);
	readonly hasElementRootAccess = this.#currentUser.asObservablePart((user) => user?.hasElementRootAccess);
	readonly hasMediaRootAccess = this.#currentUser.asObservablePart((user) => user?.hasMediaRootAccess);
	readonly isAdmin = this.#currentUser.asObservablePart((user) => user?.isAdmin);
	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode);
	readonly languages = this.#currentUser.asObservablePart((user) => user?.languages);
	readonly mediaStartNodeUniques = this.#currentUser.asObservablePart((user) => user?.mediaStartNodeUniques);
	readonly name = this.#currentUser.asObservablePart((user) => user?.name);
	readonly permissions = this.#currentUser.asObservablePart((user) => user?.permissions);
	readonly unique = this.#currentUser.asObservablePart((user) => user?.unique);
	readonly userName = this.#currentUser.asObservablePart((user) => user?.userName);

	#currentUserRepository = new UmbCurrentUserRepository(this);
	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#removeActionEventListeners();
			this.#actionEventContext = context;
			this.#addActionEventListeners();
		});

		this.observe(this.languageIsoCode, (currentLanguageIsoCode) => {
			if (!currentLanguageIsoCode) return;
			umbLocalizationRegistry.loadLanguage(currentLanguageIsoCode);
		});
	}

	#loadPromise?: Promise<void>;
	/**
	 * Loads the current user. Concurrent callers share the same in-flight promise,
	 * so awaiting `load()` always waits for `#currentUser` to be populated.
	 * @returns {Promise<void>} Resolves once the current user observable has emitted.
	 */
	public async load(): Promise<void> {
		if (!this.#loadPromise) {
			this.#loadPromise = this.#doLoad();
		}
		return this.#loadPromise;
	}

	/**
	 * Invalidates the loaded current user, so that the next call to {@link load} fetches it from the server again.
	 * Call this when the loaded user can no longer be trusted, e.g. when authorization is lost — a subsequent
	 * sign-in may be for a different user.
	 * @remarks The `#currentUser` state is deliberately NOT cleared here. The backoffice stays mounted behind
	 * the login overlay during a session timeout, and several consumers observe the unfiltered observable
	 * parts (e.g. recycle-bin conditions, sensitive-data property gating, language read-only checks) without
	 * guarding against `undefined` — clearing would tear down or read-only-flip open workspaces mid-edit for
	 * the common case of the same user signing back in. Consumers of the filtered {@link currentUser}
	 * observable hold their last value until the fresh user has been fetched and then re-evaluate.
	 */
	public invalidate(): void {
		this.#loadPromise = undefined;
	}

	async #doLoad(): Promise<void> {
		const { asObservable } = await this.#currentUserRepository.requestCurrentUser();

		if (asObservable) {
			await this.observe(
				asObservable(),
				(currentUser) => {
					this.#currentUser?.setValue(currentUser);
				},
				'observeUser',
			)
				.asPromise()
				// Ignore the error, we can assume that the flow was stopped (asPromise failed), but it does not mean that the consumption was not successful.
				.catch(() => undefined);
		}
	}

	#loadDebounced = debounce(() => {
		this.invalidate();
		this.load();
	}, 100);

	/**
	 * Checks if a user is the current user.
	 * @param userUnique The user id to check
	 * @returns True if the user is the current user, otherwise false
	 */
	async isUserCurrentUser(userUnique: string): Promise<boolean> {
		const currentUser = await firstValueFrom(this.currentUser);
		return currentUser?.unique === userUnique;
	}

	/**
	 * Checks if the current user is an admin.
	 * @returns True if the current user is an admin, otherwise false
	 */
	async isCurrentUserAdmin(): Promise<boolean> {
		const currentUser = await firstValueFrom(this.currentUser);
		return currentUser?.isAdmin ?? false;
	}

	/**
	 * Get the allowed sections for the current user
	 * @returns {Array<string> | undefined} The allowed sections for the current user
	 */
	getAllowedSection(): Array<string> | undefined {
		return this.#currentUser.getValue()?.allowedSections;
	}

	/**
	 * Get the avatar urls for the current user
	 * @returns {Array<string> | undefined} The avatar urls for the current user
	 */
	getAvatarUrls(): Array<string> | undefined {
		return this.#currentUser.getValue()?.avatarUrls;
	}

	/**
	 * Get the document start node uniques for the current user
	 * @returns {Array<UmbReferenceByUnique> | undefined} The document start node uniques for the current user
	 */
	getDocumentStartNodeUniques(): Array<UmbReferenceByUnique> | undefined {
		return this.#currentUser.getValue()?.documentStartNodeUniques;
	}

	/**
	 * Get the email for the current user
	 * @returns {string | undefined} The email for the current user
	 */
	getEmail(): string | undefined {
		return this.#currentUser.getValue()?.email;
	}

	/**
	 * Get the fallback permissions for the current user
	 * @returns {Array<string> | undefined} The fallback permissions for the current user
	 */
	getFallbackPermissions(): Array<string> | undefined {
		return this.#currentUser.getValue()?.fallbackPermissions;
	}

	/**
	 * Get if the current user has access to all languages
	 * @returns {boolean | undefined} True if the current user has access to all languages, otherwise false
	 */
	getHasAccessToAllLanguages(): boolean | undefined {
		return this.#currentUser.getValue()?.hasAccessToAllLanguages;
	}

	/**
	 * Get if the current user has access to sensitive data
	 * @returns {boolean | undefined} True if the current user has access to sensitive data, otherwise false
	 */
	getHasAccessToSensitiveData(): boolean | undefined {
		return this.#currentUser.getValue()?.hasAccessToSensitiveData;
	}

	/**
	 * Get if the current user has document root access
	 * @returns {boolean | undefined} True if the current user has document root access, otherwise false
	 */
	getHasDocumentRootAccess(): boolean | undefined {
		return this.#currentUser.getValue()?.hasDocumentRootAccess;
	}

	/**
	 * Get if the current user has media root access
	 * @returns {boolean | undefined} True if the current user has media root access, otherwise false
	 */
	getHasMediaRootAccess(): boolean | undefined {
		return this.#currentUser.getValue()?.hasMediaRootAccess;
	}

	/**
	 * Get if the current user is an admin
	 * @returns {boolean | undefined} True if the current user is an admin, otherwise false
	 */
	getIsAdmin(): boolean | undefined {
		return this.#currentUser.getValue()?.isAdmin;
	}

	/**
	 * Get the language iso code for the current user
	 * @returns {string | undefined} The language iso code for the current user
	 */
	getLanguageIsoCode(): string | undefined {
		return this.#currentUser.getValue()?.languageIsoCode;
	}

	/**
	 * Get the languages for the current user
	 * @returns {Array<string> | undefined} The languages for the current user
	 */
	getLanguages(): Array<string> | undefined {
		return this.#currentUser.getValue()?.languages;
	}

	/**
	 * Get the media start node uniques for the current user
	 * @returns {Array<UmbReferenceByUnique> | undefined} The media start node uniques for the current user
	 */
	getMediaStartNodeUniques(): Array<UmbReferenceByUnique> | undefined {
		return this.#currentUser.getValue()?.mediaStartNodeUniques;
	}

	/**
	 * Get the name for the current user
	 * @returns {string | undefined} The name for the current user
	 */
	getName(): string | undefined {
		return this.#currentUser.getValue()?.name;
	}

	/**
	 * Get the permissions for the current user
	 * @returns {unknown[] | undefined} The permissions for the current user
	 */
	getPermissions(): unknown[] | undefined {
		return this.#currentUser.getValue()?.permissions;
	}

	/**
	 * Get the unique for the current user
	 * @returns {string | undefined} The unique for the current user
	 */
	getUnique(): string | undefined {
		return this.#currentUser.getValue()?.unique;
	}

	/**
	 * Get the user name for the current user
	 * @returns {string | undefined} The user name for the current user
	 */
	getUserName(): string | undefined {
		return this.#currentUser.getValue()?.userName;
	}

	#isInGroup(groupUnique: string): boolean {
		return this.#currentUser.getValue()?.userGroupUniques.includes(groupUnique) ?? false;
	}

	#onEntityUpdatedEvent = (event: UmbEntityUpdatedEvent) => {
		const entityType = event.getEntityType();
		const unique = event.getUnique();
		if (!unique) return;

		if (entityType === UMB_USER_GROUP_ENTITY_TYPE) {
			if (this.#isInGroup(unique)) {
				this.#loadDebounced();
			}
			return;
		}

		const isCurrentUser = entityType === UMB_USER_ENTITY_TYPE && unique === this.#currentUser.getValue()?.unique;
		if (isCurrentUser) {
			this.#loadDebounced();
		}
	};

	#onEntityDeletedEvent = (event: UmbEntityDeletedEvent) => {
		if (event.getEntityType() !== UMB_USER_GROUP_ENTITY_TYPE) return;
		const unique = event.getUnique();
		if (!unique) return;
		if (this.#isInGroup(unique)) {
			this.#loadDebounced();
		}
	};

	#addActionEventListeners(): void {
		this.#actionEventContext?.addEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		this.#actionEventContext?.addEventListener(
			UmbEntityDeletedEvent.TYPE,
			this.#onEntityDeletedEvent as unknown as EventListener,
		);
	}

	#removeActionEventListeners(): void {
		this.#actionEventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		this.#actionEventContext?.removeEventListener(
			UmbEntityDeletedEvent.TYPE,
			this.#onEntityDeletedEvent as unknown as EventListener,
		);
	}

	override destroy(): void {
		this.#removeActionEventListeners();
		this.#loadDebounced.cancel();
		super.destroy();
	}
}

export default UmbCurrentUserContext;