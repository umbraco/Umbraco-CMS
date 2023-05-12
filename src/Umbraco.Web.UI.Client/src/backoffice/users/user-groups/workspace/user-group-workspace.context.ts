import { UmbUserGroupRepository } from '../repository/user-group.repository';
import { UmbUserRepository } from '../../users/repository/user.repository';
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

	#userIds = new UmbArrayState<string>([]);
	userIds = this.#userIds.asObservable();

	#userRepository: UmbUserRepository;

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbUserGroupRepository(host));

		this.#userRepository = new UmbUserRepository(host);
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

		const { data: users } = await this.#userRepository.filterCollection({
			skip: 0,
			take: 10000000,
			userGroupIds: [id],
		});

		if (!users) return;

		const ids = users.items.map((user) => user.id ?? '');

		this.#userIds.next(ids);
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

		if (this.#data) {
			//TODO: Remove all the !'s
			console.log(this.#userIds.getValue(), 'USERS');

			await this.#userRepository.setUserGroups(this.#userIds.getValue(), [this.#data!.getValue()!.id!]);
		}

		//TODO: Could we clean this code up?
		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else if (this.#data.value.id) {
			await this.repository.save(this.#data.value.id, this.#data.value);
		} else return;

		//TODO Call user repository:

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
		this.#userIds.next(keys);
	}
}
