import { UmbUserGroupDetailRepository } from '../repository/detail/index.js';
import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
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

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.UserGroup');
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async create() {
		this.resetState();
		const { data } = await this.repository.createScaffold(null);
		this.setIsNew(true);
		this.#data.setValue(data);
		return { data };
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	getUnique() {
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
	undefined,
	(context): context is UmbUserGroupWorkspaceContext => context.getEntityType?.() === 'user-group',
);
