import { UMB_USER_STORE_CONTEXT_TOKEN } from '../../users/user.store';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbEntityWorkspaceManager } from '../../../shared/components/workspace/workspace-context/entity-manager-controller';
import type { UserDetails } from '@umbraco-cms/models';

export class UmbWorkspaceUserContext extends UmbWorkspaceContext implements UmbWorkspaceEntityContextInterface<UserDetails | undefined> {

	#manager = new UmbEntityWorkspaceManager(this._host, 'user', UMB_USER_STORE_CONTEXT_TOKEN);

	public readonly data = this.#manager.state.asObservable();
	public readonly name = this.#manager.state.getObservablePart((state) => state?.name);

	// TODO: remove this magic connection, instead create the necessary methods to update parts.
	update = this.#manager.state.update;

	setName(name: string) {
		this.#manager.state.update({name: name})
	}
	getEntityType = this.#manager.getEntityType;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData;
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;

	public setPropertyValue(alias: string, value: unknown) {
		throw new Error('setPropertyValue is not implemented for UmbWorkspaceUserContext');
	}
}
