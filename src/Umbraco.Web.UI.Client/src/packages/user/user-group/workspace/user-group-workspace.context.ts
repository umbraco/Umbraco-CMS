import { UmbUserGroupDetailRepository } from '../repository/detail/index.js';
import { UmbUserRepository } from '../../user/repository/user.repository.js';
import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbUserGroupWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbUserGroupDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	public readonly repository: UmbUserGroupDetailRepository = new UmbUserGroupDetailRepository(this);

	#data = new UmbObjectState<UmbUserGroupDetailModel | undefined>(undefined);
	data = this.#data.asObservable();

	#userUniques = new UmbArrayState<string>([], (x) => x);
	userUniques = this.#userUniques.asObservable();

	#userRepository: UmbUserRepository;

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.UserGroup');

		this.#userRepository = new UmbUserRepository(host);
	}

	async create() {
		const { data } = await this.repository.createScaffold(null);
		this.setIsNew(true);
		this.#data.setValue(data);
		return { data };
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	getEntityId() {
		return this.getData()?.unique;
	}

	getEntityType(): string {
		return 'user-group';
	}

	getData() {
		return this.#data.getValue();
	}

	async save() {
		if (!this.#data.value) return;

		//TODO: Could we clean this code up?
		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else if (this.#data.value.unique) {
			await this.repository.save(this.#data.value);
		} else return;

		//TODO: This next user-group section kinda works. But it will overwrite the entire user-group list on the user.
		//TODO: instead we need to get all the users by their id's to get their user groups.
		//TODO: these user-groups need to be updated together with the new user-group id.
		//TODO: or the new user-group id needs to be removed from the existing list.

		const userUniques = this.#userUniques.getValue();
		const userGroupUniques = [this.#data.getValue()?.unique ?? ''];

		if (userUniques.length > 0 && userGroupUniques.length > 0) {
			await this.#userRepository.setUserGroups(userUniques, userGroupUniques);
		}

		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	destroy(): void {
		this.#data.destroy();
		super.destroy();
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	updateProperty<Alias extends keyof UmbUserGroupDetailModel>(alias: Alias, value: UmbUserGroupDetailModel[Alias]) {
		this.#data.update({ [alias]: value });
	}

	updateUserKeys(keys: Array<string>) {
		this.#userUniques.setValue(keys);
	}

	/**
	 * Gets the user group user permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getPermissions() {
		return this.#data.getValue()?.permissions ?? [];
	}

	/**
	 * Sets the user group user permissions.
	 * @param {Array<UmbUserPermissionModel>} permissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setPermissions(permissions: Array<UmbUserPermissionModel>) {
		this.#data.update({ permissions: permissions });
	}
}

export const UMB_USER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbUserGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbUserGroupWorkspaceContext => context.getEntityType?.() === 'user-group',
);
