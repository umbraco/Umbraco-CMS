import type { UmbUserDetailModel } from '../types.js';
import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbUserDetailRepository } from '../repository/index.js';
import { UmbUserAvatarRepository } from '../repository/avatar/index.js';
import { UMB_USER_WORKSPACE_ALIAS } from './manifests.js';
import type { UmbSaveableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UmbEditableWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbUserWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbUserDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository: UmbUserDetailRepository = new UmbUserDetailRepository(this);
	public readonly avatarRepository: UmbUserAvatarRepository = new UmbUserAvatarRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_WORKSPACE_ALIAS);
	}

	#data = new UmbObjectState<UmbUserDetailModel | undefined>(undefined);
	data = this.#data.asObservable();

	async load(unique: string) {
		const { data, asObservable } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}

		this.observe(asObservable(), (user) => this.onUserStoreChanges(user), 'umbUserStoreObserver');
	}

	/* TODO: some properties are allowed to update without saving.
		For a user properties like state will be updated when one of the entity actions are executed.
		Therefore we have to subscribe to the user store to update the state in the workspace data.
		There might be a less manual way to do this.
	*/
	onUserStoreChanges(user: UmbUserDetailModel | undefined) {
		if (!user) return;
		this.#data.update({ state: user.state, avatarUrls: user.avatarUrls });
	}

	getEntityId(): string | undefined {
		return this.getData()?.unique;
	}

	getEntityType(): string {
		return UMB_USER_ENTITY_TYPE;
	}

	getData() {
		return this.#data.getValue();
	}

	updateProperty<PropertyName extends keyof UmbUserDetailModel>(
		propertyName: PropertyName,
		value: UmbUserDetailModel[PropertyName],
	) {
		this.#data.update({ [propertyName]: value });
	}

	async save() {
		if (!this.#data.value) throw new Error('Data is missing');
		if (!this.#data.value.unique) throw new Error('Unique is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.detailRepository.create(this.#data.value);
			newData = data;
		} else {
			const { data } = await this.detailRepository.save(this.#data.value);
			newData = data;
		}

		if (newData) {
			this.#data.setValue(newData);
			this.saveComplete(newData);
		}
	}

	// TODO: implement upload progress
	uploadAvatar(file: File) {
		const unique = this.getEntityId();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.uploadAvatar(unique, file);
	}

	deleteAvatar() {
		const unique = this.getEntityId();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.deleteAvatar(unique);
	}

	destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_USER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbUserWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbUserWorkspaceContext => context.getEntityType?.() === UMB_USER_ENTITY_TYPE,
);
