import { UmbUserGroupRepository } from '../repository/user-group.repository.js';
import { UmbUserRepository } from '../../users/repository/user.repository.js';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbUserGroupWorkspaceContext
	extends UmbWorkspaceContext<UmbUserGroupRepository, UserGroupResponseModel>
	implements UmbSaveableWorkspaceContextInterface<UserGroupResponseModel | undefined>
{
	#data = new UmbObjectState<UserGroupResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	#userIds = new UmbArrayState<string>([]);
	userIds = this.#userIds.asObservable();

	#userRepository: UmbUserRepository;

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.UserGroup', new UmbUserGroupRepository(host));

		this.#userRepository = new UmbUserRepository(host);
	}

	async create() {
		const { data } = await this.repository.createScaffold(null);
		this.setIsNew(true);
		// TODO: Should the data be the base model or the presentation model?
		this.#data.next(data as unknown as UserGroupResponseModel);
		return { data };
	}

	async load(id: string) {
		const { data } = await this.repository.requestById(id);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}

		const { data: users } = await this.#userRepository.filterCollection({
			skip: 0,
			take: 10000000,
			userGroupIds: [id],
		});

		if (!users) return;

		const ids = users.items.map((user) => user.id ?? '');

		this.#userIds.next(ids);
	}

	getEntityId(): string | undefined {
		throw new Error('Method not implemented.');
	}

	getEntityType(): string {
		return 'user-group';
	}

	getData(): UserGroupResponseModel | undefined {
		throw new Error('Method not implemented.');
	}

	async save() {
		if (!this.#data.value) return;

		//TODO: Could we clean this code up?
		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else if (this.#data.value.id) {
			await this.repository.save(this.#data.value.id, this.#data.value);
		} else return;

		//TODO: This next user-group section kinda works. But it will overwrite the entire user-group list on the user.
		//TODO: instead we need to get all the users by their id's to get their user groups.
		//TODO: these user-groups need to be updated together with the new user-group id.
		//TODO: or the new user-group id needs to be removed from the existing list.

		const userIds = this.#userIds.getValue();
		const userGroupIds = [this.#data.getValue()?.id ?? ''];

		if (userIds.length > 0 && userGroupIds.length > 0) {
			await this.#userRepository.setUserGroups(userIds, userGroupIds);
		}

		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	destroy(): void {
		this.#data.complete();
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	updateProperty<Alias extends keyof UserGroupResponseModel>(alias: Alias, value: UserGroupResponseModel[Alias]) {
		this.#data.update({ [alias]: value });
	}

	updateUserKeys(keys: Array<string>) {
		this.#userIds.next(keys);
	}

	/**
	 * Sets the user group default permissions.
	 * @param {Array<string>} permissionAliases
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setDefaultPermissions(permissionAliases: Array<string>) {
		this.#data.update({ permissions: permissionAliases });
	}

	/**
	 * Gets the user group default permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getDefaultPermissions() {
		return this.#data.getValue()?.permissions ?? [];
	}

	/**
	 * Allows a default permission on the user group.
	 * @param {string} permissionAlias
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	allowDefaultPermission(permissionAlias: string) {
		const permissions = this.#data.getValue()?.permissions ?? [];
		const newValue = [...permissions, permissionAlias];
		this.#data.update({ permissions: newValue });
	}

	/**
	 * Disallows a default permission on the user group.
	 * @param {string} permissionAlias
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	disallowDefaultPermission(permissionAlias: string) {
		const permissions = this.#data.getValue()?.permissions ?? [];
		const newValue = permissions.filter((alias) => alias !== permissionAlias);
		this.#data.update({ permissions: newValue });
	}
}

export const UMB_USER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbUserGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	(context): context is UmbUserGroupWorkspaceContext => context.getEntityType?.() === 'user-group',
);
