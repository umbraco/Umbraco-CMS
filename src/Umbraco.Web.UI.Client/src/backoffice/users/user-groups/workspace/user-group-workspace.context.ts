import { UmbUserGroupRepository } from '../repository/user-group.repository';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UserGroupResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserGroupWorkspaceContext
	extends UmbWorkspaceContext<UmbUserGroupRepository, UserGroupResponseModel>
	implements UmbEntityWorkspaceContextInterface<UserGroupResponseModel | undefined>
{
	#data = new UmbObjectState<UserGroupResponseModel | undefined>(undefined);
	data = this.#data.asObservable();

	#userKeys = new UmbArrayState<string>([]);
	userKeys = this.#userKeys.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbUserGroupRepository(host));
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold(null);
		this.setIsNew(true);
		// TODO: Should the data be the base model or the presentation model?
		this.#data.next(data as unknown as UserGroupResponseModel);
		return { data };
	}

	async load(id: string) {
		const { data } = await this.repository.requestById(id);
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	getEntityId(): string | undefined {
		throw new Error('Method not implemented.');
	}
	getEntityType(): string {
		throw new Error('Method not implemented.');
	}
	getData(): UserGroupResponseModel | undefined {
		throw new Error('Method not implemented.');
	}
	async save() {
		if (!this.#data.value) return;

		//TODO: Could we clean this code up?
		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else if (this.#data.value.id) {
			await this.repository.save(this.#data.value.id, this.#data.value);
		} else return;

		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	destroy(): void {
		this.#data.complete();
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	updateProperty<Alias extends keyof UserGroupResponseModel>(alias: Alias, value: UserGroupResponseModel[Alias]) {
		this.#data.update({ [alias]: value });
	}

	updateUserKeys(keys: Array<string>) {
		this.#userKeys.next(keys);
	}
}
