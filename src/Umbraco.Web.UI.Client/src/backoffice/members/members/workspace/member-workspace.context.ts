import { UmbEntityWorkspaceManager } from '../../../shared/components/workspace/workspace-context/entity-manager-controller';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UMB_MEMBER_DETAIL_STORE_CONTEXT_TOKEN } from '../member.detail.store';
import type { MemberDetails } from '@umbraco-cms/models';

export class UmbWorkspaceMemberContext
	extends UmbWorkspaceContext
	implements UmbWorkspaceEntityContextInterface<MemberDetails | undefined>
{
	#manager = new UmbEntityWorkspaceManager(this._host, 'member', UMB_MEMBER_DETAIL_STORE_CONTEXT_TOKEN);

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
