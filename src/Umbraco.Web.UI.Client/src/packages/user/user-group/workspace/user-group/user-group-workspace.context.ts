import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS, type UmbUserGroupDetailRepository } from '../../repository/index.js';
import { UMB_USER_GROUP_ENTITY_TYPE, UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbUserGroupWorkspaceEditorElement } from './user-group-workspace-editor.element.js';
import { UMB_USER_GROUP_WORKSPACE_ALIAS } from './constants.js';
import {
	UmbEntityNamedDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
import { UserGroupService, UserService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

export class UmbUserGroupWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbUserGroupDetailModel, UmbUserGroupDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly alias = this._data.createObservablePartOfCurrent((data) => data?.alias || '');
	readonly aliasCanBeChanged = this._data.createObservablePartOfCurrent((data) => data?.aliasCanBeChanged);
	readonly icon = this._data.createObservablePartOfCurrent((data) => data?.icon || null);
	readonly sections = this._data.createObservablePartOfCurrent((data) => data?.sections || []);
	readonly languages = this._data.createObservablePartOfCurrent((data) => data?.languages || []);
	readonly hasAccessToAllLanguages = this._data.createObservablePartOfCurrent(
		(data) => data?.hasAccessToAllLanguages || false,
	);
	readonly documentStartNode = this._data.createObservablePartOfCurrent((data) => data?.documentStartNode || null);
	readonly documentRootAccess = this._data.createObservablePartOfCurrent((data) => data?.documentRootAccess || false);
	readonly mediaStartNode = this._data.createObservablePartOfCurrent((data) => data?.mediaStartNode || null);
	readonly mediaRootAccess = this._data.createObservablePartOfCurrent((data) => data?.mediaRootAccess || false);
	readonly fallbackPermissions = this._data.createObservablePartOfCurrent((data) => data?.fallbackPermissions || []);
	readonly permissions = this._data.createObservablePartOfCurrent((data) => data?.permissions || []);
	readonly description = this._data.createObservablePartOfCurrent((data) => data?.description || '');
	
	#persistedUserUniques: string[] = [];
	readonly #userUniquesState = new UmbArrayState<string>([], (v) => v);
	readonly userUniques = this.#userUniquesState.asObservable();

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_USER_GROUP_WORKSPACE_ALIAS,
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			detailRepositoryAlias: UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS,
		});

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (context) => {
			this.#notificationContext = context;
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbUserGroupWorkspaceEditorElement,
				setup: async () => {
					await this.createScaffold({ parent: { entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbUserGroupWorkspaceEditorElement,
				setup: (component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override async load(unique: string) {
		const result = await super.load(unique);
		if (!result.error) {
			await this.#loadUsers(unique);
		}
		return result;
	}

	async #loadUsers(unique: string) {
		const { data } = await tryExecute(
			this,
			UserService.getFilterUser({ query: { userGroupIds: [unique], take: 10000 } }),
		);
		const uniques = data?.items.map((u) => u.id) ?? [];
		this.#persistedUserUniques = [...uniques];
		this.#userUniquesState.setValue(uniques);
	}

	/**
	 * Sets the pending user uniques for this group (client-side only until Save).
	 * Also sets the dirty flag so the workspace knows there are unpersisted changes.
	 * @param {Array<string>} uniques
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setUserUniques(uniques: Array<string>) {
		this.#userUniquesState.setValue(uniques);
	}

	getHasUserChanges() {
		const result = jsonStringComparison(this.#persistedUserUniques, this.#userUniquesState.getValue()) === false;
		return result;
	}

	override getHasUnpersistedChanges(): boolean {
		return super.getHasUnpersistedChanges() || this.getHasUserChanges();
	}

	override async submit() {
		if (this.getIsNew()) {
			// For new groups: create group first (so it exists on server), then add users.
			await super.submit();
			await this.#persistUserChanges();
		} else {
			// For existing groups
			await super.submit();
			await this.#persistUserChanges();
		}
	}

	async #persistUserChanges() {
		const unique = this.getUnique();
		if (!this.getHasUserChanges() || !unique) return;

		const pending = this.#userUniquesState.getValue();
		const toAdd = pending.filter((u) => !this.#persistedUserUniques.includes(u));
		const toRemove = this.#persistedUserUniques.filter((u) => !pending.includes(u));

		// Run add and remove in parallel; track whether either call errored.
		// Only update local state when all API calls succeeded.
		const [addError, removeError] = await Promise.all([
			this.#addUsersToGroup(unique, toAdd),
			this.#removeUsersFromGroup(unique, toRemove),
		]);

		if (addError) {
			this.#notificationContext?.peek('danger', {
				data: {headline: 'An error occurred', message: 'Can not add users to the group.' },
			});
		}

		if (removeError) {
			this.#notificationContext?.peek('danger', {
				data: {headline: 'An error occurred', message: 'Can not remove users from the group.' },
			});
		}

		if (!addError && !removeError) {
			this.#persistedUserUniques = [...pending];
		}
	}

	async #addUsersToGroup(unique: string, userIds: string[]): Promise<boolean> {
		if (!userIds.length) return false;
		const { error } = await tryExecute(
			this,
			UserGroupService.postUserGroupByIdUsers({
				path: { id: unique },
				body: userIds.map((id) => ({ id })),
			}),
		);
		return !!error;
	}

	async #removeUsersFromGroup(unique: string, userIds: string[]): Promise<boolean> {
		if (!userIds.length) return false;
		const { error } = await tryExecute(
			this,
			UserGroupService.deleteUserGroupByIdUsers({
				path: { id: unique },
				body: userIds.map((id) => ({ id })),
			}),
		);
		return !!error;
	}

	override resetState() {
		super.resetState();
		this.#persistedUserUniques = [];
		this.#userUniquesState.setValue([]);
	}

	updateProperty<Alias extends keyof UmbUserGroupDetailModel>(alias: Alias, value: UmbUserGroupDetailModel[Alias]) {
		this._data.updateCurrent({ [alias]: value });
	}

	/**
	 * Gets the user group user permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getPermissions() {
		return this._data.getCurrent()?.permissions ?? [];
	}

	/**
	 * Sets the user group user permissions.
	 * @param {Array<UmbUserPermissionModel>} permissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setPermissions(permissions: Array<UmbUserPermissionModel>) {
		this._data.updateCurrent({ permissions: permissions });
	}

	/**
	 * Gets the user group fallback permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getFallbackPermissions() {
		return this._data.getCurrent()?.fallbackPermissions ?? [];
	}

	/**
	 * Sets the user group fallback permissions.
	 * @param {Array<string>} fallbackPermissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setFallbackPermissions(fallbackPermissions: Array<string>) {
		this._data.updateCurrent({ fallbackPermissions });
	}

	/**
	 * Sets the description
	 * @param {string} description - The description
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setDescription(description: string) {
		this._data.updateCurrent({ description });
	}
}

export { UmbUserGroupWorkspaceContext as api };
