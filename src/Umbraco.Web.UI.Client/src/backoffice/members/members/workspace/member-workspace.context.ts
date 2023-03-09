import { UmbEntityWorkspaceManager } from '../../../shared/components/workspace/workspace-context/entity-manager-controller';
import { UmbWorkspaceContext } from '../../../shared/components/workspace/workspace-context/workspace-context';
import { UmbWorkspaceEntityContextInterface } from '../../../shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UMB_MEMBER_STORE_CONTEXT_TOKEN } from '../member.detail.store';
import { UmbMemberRepository } from '../repository/member.repository';
import type { MemberDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export class UmbWorkspaceMemberContext
	extends UmbWorkspaceContext<UmbMemberRepository>
	implements UmbWorkspaceEntityContextInterface<MemberDetails | undefined>
{
	#manager = new UmbEntityWorkspaceManager(this.host, 'member', UMB_MEMBER_STORE_CONTEXT_TOKEN);

	public readonly data = this.#manager.state.asObservable();
	public readonly name = this.#manager.state.getObservablePart((state) => state?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbMemberRepository(host));
	}

	setPropertyValue(alias: string, value: string) {
		return;
	}

	setName(name: string) {
		this.#manager.state.update({ name });
	}

	getEntityType = this.#manager.getEntityType;
	getUnique = this.#manager.getEntityKey;
	getEntityKey = this.#manager.getEntityKey;
	getStore = this.#manager.getStore;
	getData = this.#manager.getData as any; // TODO: fix this type mismatch, but this will be done when we move to repositories.
	load = this.#manager.load;
	create = this.#manager.create;
	save = this.#manager.save;
	destroy = this.#manager.destroy;
}
