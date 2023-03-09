import { UmbEntityWorkspaceManager } from '../../../shared/components/workspace/workspace-context/entity-manager-controller';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UMB_USER_GROUP_STORE_CONTEXT_TOKEN } from '../repository/user-group.store';
import { UmbUserGroupRepository } from '../repository/user-group.repository';
import type { UserGroupDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbWorkspaceUserGroupContext
	extends UmbWorkspaceContext<UmbUserGroupRepository>
	implements UmbWorkspaceEntityContextInterface<UserGroupDetails | undefined>
{
	#manager = new UmbEntityWorkspaceManager<typeof UMB_USER_GROUP_STORE_CONTEXT_TOKEN.TYPE>(
		this.host,
		'user-group',
		UMB_USER_GROUP_STORE_CONTEXT_TOKEN
	);

	public readonly data = this.#manager.state.asObservable();
	public readonly name = this.#manager.state.getObservablePart((state) => state?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbUserGroupRepository(host));
	}

	setName(name: string) {
		this.#manager.state.update({ name: name });
	}
	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData as any; // TODO: fix type mismatch, but this will be done when we move to repositories.
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceUserGroupContext');
	}
}
