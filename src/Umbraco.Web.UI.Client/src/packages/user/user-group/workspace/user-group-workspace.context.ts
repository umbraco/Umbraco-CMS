import { UmbUserGroupDetailRepository } from '../repository/detail/index.js';
import type { UmbUserGroupDetailModel } from '../types.js';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
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

	readonly name = this.#data.asObservablePart((data) => data?.name || '');
	readonly icon = this.#data.asObservablePart((data) => data?.icon || null);
	readonly sections = this.#data.asObservablePart((data) => data?.sections || []);
	readonly languages = this.#data.asObservablePart((data) => data?.languages || []);
	readonly hasAccessToAllLanguages = this.#data.asObservablePart((data) => data?.hasAccessToAllLanguages || false);
	readonly documentStartNode = this.#data.asObservablePart((data) => data?.documentStartNode || null);
	readonly documentRootAccess = this.#data.asObservablePart((data) => data?.documentRootAccess || false);
	readonly mediaStartNode = this.#data.asObservablePart((data) => data?.mediaStartNode || null);
	readonly mediaRootAccess = this.#data.asObservablePart((data) => data?.mediaRootAccess || false);
	readonly fallbackPermissions = this.#data.asObservablePart((data) => data?.fallbackPermissions || []);
	readonly permissions = this.#data.asObservablePart((data) => data?.permissions || []);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.UserGroup');
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async create() {
		this.resetState();
		const { data } = await this.repository.createScaffold();
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

	/**
	 * Gets the user group fallback permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getFallbackPermissions() {
		return this.#data.getValue()?.fallbackPermissions ?? [];
	}

	/**
	 * Sets the user group fallback permissions.
	 * @param {Array<string>} fallbackPermissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setFallbackPermissions(fallbackPermissions: Array<string>) {
		this.#data.update({ fallbackPermissions });
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
