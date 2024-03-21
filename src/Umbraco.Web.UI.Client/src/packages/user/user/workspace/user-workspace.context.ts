import type { UmbUserDetailModel, UmbUserStateEnum } from '../types.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbUserDetailRepository } from '../repository/index.js';
import { UmbUserAvatarRepository } from '../repository/avatar/index.js';
import { UMB_USER_WORKSPACE_ALIAS } from './manifests.js';
import { UmbUserWorkspaceEditorElement } from './user-workspace-editor.element.js';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbSaveableWorkspaceContextBase, UmbWorkspaceRouteManager } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

type EntityType = UmbUserDetailModel;

export class UmbUserWorkspaceContext
	extends UmbSaveableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository: UmbUserDetailRepository = new UmbUserDetailRepository(this);
	public readonly avatarRepository: UmbUserAvatarRepository = new UmbUserAvatarRepository(this);

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	readonly data = this.#currentData.asObservable();
	readonly state = this.#currentData.asObservablePart((x) => x?.state);
	readonly unique = this.#currentData.asObservablePart((x) => x?.unique);

	readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_WORKSPACE_ALIAS);
		this.routes.setRoutes([
			{
				path: ':id',
				component: UmbUserWorkspaceEditorElement,
				setup: (component, info) => {
					const id = info.match.params.id;
					this.load(id);
				},
			},
		]);
	}

	async load(unique: string) {
		const { data, asObservable } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#persistedData.update(data);
			this.#currentData.update(data);
		}

		this.observe(asObservable(), (user) => this.onUserStoreChanges(user), 'umbUserStoreObserver');
	}

	/* TODO: some properties are allowed to update without saving.
		For a user properties like state will be updated when one of the entity actions are executed.
		Therefore we have to subscribe to the user store to update the state in the workspace data.
		There might be a less manual way to do this.
	*/
	onUserStoreChanges(user: EntityType | undefined) {
		if (!user) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/user-management');
			return;
		}
		this.#currentData.update({ state: user.state, avatarUrls: user.avatarUrls });
	}

	getUnique(): string | undefined {
		return this.getData()?.unique;
	}
	getState(): UmbUserStateEnum | null | undefined {
		return this.getData()?.state;
	}

	getEntityType(): string {
		return UMB_USER_ENTITY_TYPE;
	}

	getData() {
		return this.#currentData.getValue();
	}

	updateProperty<PropertyName extends keyof EntityType>(propertyName: PropertyName, value: EntityType[PropertyName]) {
		this.#currentData.update({ [propertyName]: value });
	}

	async save() {
		if (!this.#currentData.value) throw new Error('Data is missing');
		if (!this.#currentData.value.unique) throw new Error('Unique is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.detailRepository.create(this.#currentData.value);
			newData = data;
		} else {
			const { data } = await this.detailRepository.save(this.#currentData.value);
			newData = data;
		}

		if (newData) {
			this.#persistedData.setValue(newData);
			this.#currentData.setValue(newData);
			this.setIsNew(false);
			this.workspaceComplete(newData);
		}
	}

	// TODO: implement upload progress
	uploadAvatar(file: File) {
		const unique = this.getUnique();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.uploadAvatar(unique, file);
	}

	deleteAvatar() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.deleteAvatar(unique);
	}

	destroy(): void {
		this.#persistedData.destroy();
		this.#currentData.destroy();
		this.detailRepository.destroy();
		this.avatarRepository.destroy();
		super.destroy();
	}
}

export { UmbUserWorkspaceContext as api };
