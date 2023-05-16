import { UmbUserRepository } from '../repository/user.repository';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UpdateUserRequestModel, UserResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserWorkspaceContext
	extends UmbWorkspaceContext<UmbUserRepository, UserResponseModel>
	implements UmbEntityWorkspaceContextInterface<UserResponseModel | undefined>
{
	constructor(host: UmbControllerHostElement) {
		super(host, new UmbUserRepository(host));
	}

	#data = new UmbObjectState<UserResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	async load(id: string) {
		console.log('load');

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

	updateProperty<PropertyName extends keyof UserResponseModel>(
		propertyName: PropertyName,
		value: UserResponseModel[PropertyName]
	) {
		this.#data.update({ [propertyName]: value });
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		console.log('save', this.#data.value, this.getIsNew());

		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else {
			//TODO: temp hack: why does the response model allow for nulls but not the request model?
			await this.repository.save(this.#data.value.id, this.#data.value as UpdateUserRequestModel);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	destroy(): void {
		this.#data.complete();
	}
}
