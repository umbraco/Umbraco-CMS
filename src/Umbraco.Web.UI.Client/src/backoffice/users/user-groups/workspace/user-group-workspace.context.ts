import { UmbUserGroupRepository } from '../repository/user-group.repository';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UserGroupPresentationModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserGroupWorkspaceContext
	extends UmbWorkspaceContext<UmbUserGroupRepository, UserGroupPresentationModel>
	implements UmbEntityWorkspaceContextInterface<UserGroupPresentationModel | undefined>
{
	#data = new UmbObjectState<UserGroupPresentationModel | undefined>(undefined);
	data = this.#data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, new UmbUserGroupRepository(host));
	}

	async load(id: string) {
		console.log('load');

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
	getData(): UserGroupPresentationModel | undefined {
		throw new Error('Method not implemented.');
	}
	save(): Promise<void> {
		throw new Error('Method not implemented.');
	}
	destroy(): void {
		throw new Error('Method not implemented.');
	}

	updateProperty<Alias extends keyof UserGroupPresentationModel>(
		alias: Alias,
		value: UserGroupPresentationModel[Alias]
	) {
		this.#data.update({ [alias]: value });
	}
}
