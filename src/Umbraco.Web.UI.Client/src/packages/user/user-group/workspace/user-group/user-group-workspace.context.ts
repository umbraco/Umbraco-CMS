import { UmbUserGroupDetailRepository } from '../../repository/detail/index.js';
import type { UmbUserGroupDetailModel } from '../../types.js';
import { UmbUserGroupWorkspaceEditorElement } from './user-group-workspace-editor.element.js';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbUserGroupDetailModel>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	//
	public readonly repository: UmbUserGroupDetailRepository = new UmbUserGroupDetailRepository(this);

	#data = new UmbObjectState<UmbUserGroupDetailModel | undefined>(undefined);
	data = this.#data.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly name = this.#data.asObservablePart((data) => data?.name || '');
	readonly alias = this.#data.asObservablePart((data) => data?.alias || '');
	readonly aliasCanBeChanged = this.#data.asObservablePart((data) => data?.aliasCanBeChanged);
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

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbUserGroupWorkspaceEditorElement,
				setup: () => {
					this.create();

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

	protected override resetState(): void {
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

	async submit() {
		if (!this.#data.value) throw new Error('Data is missing');

		if (this.getIsNew()) {
			const { error, data } = await this.repository.create(this.#data.value);
			if (data) {
				this.#data.setValue(data);
				this.setIsNew(false);
			}
			if (error) throw new Error(error.message);
		} else if (this.#data.value.unique) {
			const { error, data } = await this.repository.save(this.#data.value);
			if (data) {
				this.#data.setValue(data);
			}
			if (error) throw new Error(error.message);
		}
	}

	override destroy(): void {
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

export { UmbUserGroupWorkspaceContext as api };
