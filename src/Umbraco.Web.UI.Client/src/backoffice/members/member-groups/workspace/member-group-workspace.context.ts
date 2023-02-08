import { UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN } from '../member-group.detail.store';
import { UmbWorkspaceEntityContextInterface } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbEntityWorkspaceManager } from '../../../../backoffice/shared/components/workspace/workspace-context/entity-manager-controller';
import { UmbWorkspaceContext } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-context';
import type { MemberGroupDetails } from '@umbraco-cms/models';

export class UmbWorkspaceMemberGroupContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<MemberGroupDetails | undefined>
{
	#manager = new UmbEntityWorkspaceManager(this._host, 'memberGroup', UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN);

	public readonly data = this.#manager.state.asObservable();
	public readonly name = this.#manager.state.getObservablePart((state) => state?.name);

	setPropertyValue(alias: string, value: string) {
		return;
	}

	setName(name: string) {
		this.#manager.state.update({name});
	}

	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData;
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;
}
