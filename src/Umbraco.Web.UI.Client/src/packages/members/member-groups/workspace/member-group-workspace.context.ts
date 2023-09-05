import { UmbMemberGroupRepository } from '../repository/member-group.repository.js';
import type { MemberGroupDetails } from '../types.js';
import { UmbEntityWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

type EntityType = MemberGroupDetails;
export class UmbMemberGroupWorkspaceContext
	extends UmbWorkspaceContext<UmbMemberGroupRepository, EntityType>
	implements UmbEntityWorkspaceContextInterface<EntityType | undefined>
{
	#data = new UmbObjectState<EntityType | undefined>(undefined);
	data = this.#data.asObservable();
	name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MemberGroup', new UmbMemberGroupRepository(host));
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
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

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.#data.next(data);
		}
	}

	async create() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	async save() {
		if (!this.#data.value) return;
		await this.repository.save(this.#data.value.id, this.#data.value);
		this.setIsNew(true);
	}

	public destroy(): void {
		this.#data.complete();
	}
}



export const UMB_MEMBER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<UmbEntityWorkspaceContextInterface, UmbMemberGroupWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbMemberGroupWorkspaceContext => context.getEntityType?.() === 'member-group'
);
