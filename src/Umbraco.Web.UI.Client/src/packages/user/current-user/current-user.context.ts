import type { UmbCurrentUserModel } from './types.js';
import { UmbCurrentUserRepository } from './repository/current-user.repository.js';
import { UMB_CURRENT_USER_CONTEXT } from './current-user.context.token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { filter, firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbLocalizationRegistry } from '@umbraco-cms/backoffice/localization';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';
import { ensurePathEndsWithSlash } from '@umbraco-cms/backoffice/utils';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';

export class UmbCurrentUserContext extends UmbContextBase<UmbCurrentUserContext> {
	#currentUser = new UmbObjectState<UmbCurrentUserModel | undefined>(undefined);
	readonly currentUser = this.#currentUser.asObservable().pipe(filter((user) => !!user));
	readonly allowedSections = this.#currentUser.asObservablePart((user) => user?.allowedSections);
	readonly avatarUrls = this.#currentUser.asObservablePart((user) => user?.avatarUrls);
	readonly documentStartNodeUniques = this.#currentUser.asObservablePart((user) => user?.documentStartNodeUniques);
	readonly email = this.#currentUser.asObservablePart((user) => user?.email);
	readonly fallbackPermissions = this.#currentUser.asObservablePart((user) => user?.fallbackPermissions);
	readonly hasAccessToAllLanguages = this.#currentUser.asObservablePart((user) => user?.hasAccessToAllLanguages);
	readonly hasAccessToSensitiveData = this.#currentUser.asObservablePart((user) => user?.hasAccessToSensitiveData);
	readonly hasDocumentRootAccess = this.#currentUser.asObservablePart((user) => user?.hasDocumentRootAccess);
	readonly hasMediaRootAccess = this.#currentUser.asObservablePart((user) => user?.hasMediaRootAccess);
	readonly isAdmin = this.#currentUser.asObservablePart((user) => user?.isAdmin);
	readonly languageIsoCode = this.#currentUser.asObservablePart((user) => user?.languageIsoCode);
	readonly languages = this.#currentUser.asObservablePart((user) => user?.languages);
	readonly mediaStartNodeUniques = this.#currentUser.asObservablePart((user) => user?.mediaStartNodeUniques);
	readonly name = this.#currentUser.asObservablePart((user) => user?.name);
	readonly permissions = this.#currentUser.asObservablePart((user) => user?.permissions);
	readonly unique = this.#currentUser.asObservablePart((user) => user?.unique);
	readonly userName = this.#currentUser.asObservablePart((user) => user?.userName);

	#authContext?: typeof UMB_AUTH_CONTEXT.TYPE;
	#currentUserRepository = new UmbCurrentUserRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT);

		this.consumeContext(UMB_AUTH_CONTEXT, (instance) => {
			this.#authContext = instance;
			this.#observeIsAuthorized();
		});

		this.observe(this.languageIsoCode, (currentLanguageIsoCode) => {
			if (!currentLanguageIsoCode) return;
			umbLocalizationRegistry.loadLanguage(currentLanguageIsoCode);
		});
	}

	/**
	 * Loads the current user
	 */
	async load() {
		const { asObservable } = await this.#currentUserRepository.requestCurrentUser();

		if (asObservable) {
			await this.observe(asObservable(), (currentUser) => {
				this.#currentUser?.setValue(currentUser);
				this.#redirectToFirstAllowedSectionIfNeeded();
			}).asPromise();
		}
	}

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
	 * @returns {Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel> | undefined} The permissions for the current user
	 */
	getPermissions() {
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

	#observeIsAuthorized() {
		if (!this.#authContext) return;
		this.observe(this.#authContext.isAuthorized, (isAuthorized) => {
			if (isAuthorized) {
				this.load();
			}
		});
	}

	async #redirectToFirstAllowedSectionIfNeeded() {
		const url = new URL(window.location.href);

		const serverContext = await this.getContext(UMB_SERVER_CONTEXT);
		if (!serverContext) {
			throw new Error('Server context not available');
		}
		const backofficePath = serverContext.getBackofficePath();

		if (url.pathname === backofficePath || url.pathname === ensurePathEndsWithSlash(backofficePath)) {
			const sectionManifest = await this.#firstAllowedSection();
			if (!sectionManifest) return;

			const fallbackSectionPath = UMB_SECTION_PATH_PATTERN.generateLocal({
				sectionName: sectionManifest.meta.pathname,
			});

			history.pushState(null, '', fallbackSectionPath);
		}
	}

	async #firstAllowedSection() {
		const currentUser = this.#currentUser.getValue();
		if (!currentUser) return;

		/* TODO: this solution is not bullet proof as we still rely on the "correct" section to be registered at this point in time so we can get the path.
		 It probably would have been better if we used the section alias instead as the path.
		 Then we would have it available at all times and it also ensured a unique path. */
		const sections = await this.observe(
			umbExtensionsRegistry.byTypeAndAliases('section', currentUser.allowedSections),
			() => {},
		).asPromise();

		return sections[0];
	}
}

export default UmbCurrentUserContext;
