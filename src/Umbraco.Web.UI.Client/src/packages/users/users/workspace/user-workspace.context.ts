import { UmbUserRepository } from '../repository/user.repository.js';
import { type UmbUserDetail } from '../index.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UpdateUserRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextConsumerController, UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbUserWorkspaceContext
	extends UmbWorkspaceContext<UmbUserRepository, UmbUserDetail>
	implements UmbEntityWorkspaceContextInterface<UmbUserDetail | undefined>
{
	#authContext?: typeof UMB_AUTH.TYPE;

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.User', new UmbUserRepository(host));

		new UmbContextConsumerController(host, UMB_AUTH, (auth) => {
			this.#authContext = auth;
		});
	}

	#data = new UmbObjectState<UmbUserDetail | undefined>(undefined);
	data = this.#data.asObservable();

	async load(id: string) {
		const { data } = await this.repository.requestById(id);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	getEntityId(): string | undefined {
		return this.getData()?.id || '';
	}

	getEntityType(): string {
		return 'user';
	}

	getData() {
		return this.#data.getValue();
	}

	updateProperty<PropertyName extends keyof UmbUserDetail>(
		propertyName: PropertyName,
		value: UmbUserDetail[PropertyName]
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
		if (!this.#authContext) return;
		const currentUser = await firstValueFrom(this.#authContext.currentUser);
		if (currentUser?.id === savedUserId) {
			await this.#authContext.fetchCurrentUser();
		}
	}

	destroy(): void {
		this.#data.complete();
	}
}

export const UMB_USER_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface, UmbUserWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbUserWorkspaceContext => context.getEntityType?.() === 'user'
);
