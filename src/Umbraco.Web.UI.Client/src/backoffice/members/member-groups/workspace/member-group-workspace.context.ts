import { UmbWorkspaceEntityContextInterface } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-entity-context.interface';
import { UmbWorkspaceContext } from '../../../../backoffice/shared/components/workspace/workspace-context/workspace-context';
import { UmbMemberGroupRepository } from '../repository/member-group.repository';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ObjectState } from '@umbraco-cms/observable-api';

type EntityType = MemberGroupDetails;
export class UmbWorkspaceMemberGroupContext
	extends UmbWorkspaceContext<UmbMemberGroupRepository>
	implements UmbWorkspaceEntityContextInterface<EntityType | undefined>
{
	#data = new ObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.getObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostInterface) {
		super(host, new UmbMemberGroupRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityKey() {
		return this.getData()?.key || '';
	}

	getEntityType() {
		return 'member-group';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setPropertyValue(alias: string, value: string) {
		// Not implemented for this context - member groups have no properties for editing
		return;
	}

	async load(entityKey: string) {
		const { data } = await this.repository.requestByKey(entityKey);
		if (data) {
			this.#data.next(data);
		}
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		await this.repository.save(this.#data.value);
		this.setIsNew(true);
	}

	public destroy(): void {
		this.#data.complete();
	}
}
