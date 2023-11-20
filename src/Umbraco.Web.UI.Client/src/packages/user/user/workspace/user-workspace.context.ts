import { UmbUserRepository } from '../repository/user.repository.js';
import { UMB_USER_ENTITY_TYPE, type UmbUserDetail } from '../index.js';
import {
	UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UpdateUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbUserWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbUserRepository, UmbUserDetail>
	implements UmbSaveableWorkspaceContextInterface<UmbUserDetail | undefined>
{
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.User', new UmbUserRepository(host));

		new UmbContextConsumerController(host, UMB_CURRENT_USER_CONTEXT, (instance) => {
			this.#currentUserContext = instance;
		});
	}

	#data = new UmbObjectState<UmbUserDetail | undefined>(undefined);
	data = this.#data.asObservable();

	async load(id: string) {
		const { data, asObservable } = await this.repository.requestById(id);
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
	onUserStoreChanges(user: UmbUserDetail) {
		if (!user) return;
		this.#data.update({ state: user.state });
	}

	getEntityId(): string | undefined {
		return this.getData()?.id || '';
	}

	getEntityType(): string {
		return UMB_USER_ENTITY_TYPE;
	}

	getData() {
		return this.#data.getValue();
	}

	updateProperty<PropertyName extends keyof UmbUserDetail>(
		propertyName: PropertyName,
		value: UmbUserDetail[PropertyName],
	) {
		this.#data.update({ [propertyName]: value });
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else {
			//TODO: temp hack: why does the response model allow for nulls but not the request model?
			await this.repository.save(this.#data.value.id, this.#data.value as UpdateUserRequestModel);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);

		// If we are saving the current user, we need to refetch it
		await this.#reloadCurrentUser(this.#data.value.id);
	}

	async #reloadCurrentUser(savedUserId: string): Promise<void> {
		if (!this.#currentUserContext) return;
		const currentUser = await firstValueFrom(this.#currentUserContext.currentUser);
		if (currentUser?.id === savedUserId) {
			await this.#currentUserContext.requestCurrentUser();
		}
	}

	// TODO: implement upload progress
	async uploadAvatar(file: File) {
		const id = this.getEntityId();
		if (!id) throw new Error('Id is missing');
		return this.repository.uploadAvatar(id, file);
	}

	async deleteAvatar() {
		const id = this.getEntityId();
		if (!id) throw new Error('Id is missing');
		return this.repository.deleteAvatar(id);
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
	(context): context is UmbUserWorkspaceContext => context.getEntityType?.() === UMB_USER_ENTITY_TYPE,
);
