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
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

type EntityType = UmbUserDetailModel;

export class UmbUserWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository: UmbUserDetailRepository = new UmbUserDetailRepository(this);
	public readonly avatarRepository: UmbUserAvatarRepository = new UmbUserAvatarRepository(this);

	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_USER_WORKSPACE_ALIAS);

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
		});
	}

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	data = this.#currentData.asObservable();

	async load(unique: string) {
		const { data, asObservable } = await this.detailRepository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
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
		if (!user) return;
		this.#currentData.update({ state: user.state });
	}

	getUnique(): string | undefined {
		return this.getData()?.unique;
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

		if (this.getIsNew()) {
			await this.detailRepository.create(this.#currentData.value);
		} else {
			await this.detailRepository.save(this.#currentData.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);

		// If we are saving the current user, we need to refetch it
		await this.#reloadCurrentUser(this.#currentData.value.unique);
	}

	async #reloadCurrentUser(savedUserUnique: string): Promise<void> {
		if (!this.#currentUserContext) return;
		const currentUser = await firstValueFrom(this.#currentUserContext.currentUser);
		if (currentUser?.unique === savedUserUnique) {
			await this.#currentUserContext.requestCurrentUser();
		}
	}

	// TODO: implement upload progress
	async uploadAvatar(file: File) {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		return this.avatarRepository.uploadAvatar(unique, file);
	}

	async deleteAvatar() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Unique is missing');
		return this.avatarRepository.deleteAvatar(unique);
	}

	destroy(): void {
		this.#persistedData.destroy();
		this.#currentData.destroy();
		super.destroy();
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
