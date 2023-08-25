import { UmbMemberTypeRepository } from '../repository/member-type.repository.js';
import { UmbSaveableWorkspaceContextInterface, UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO => use correct tpye
type EntityType = any;

export class UmbMemberTypeWorkspaceContext
	extends UmbWorkspaceContext<UmbMemberTypeRepository, EntityType>
	implements UmbSaveableWorkspaceContextInterface<EntityType | undefined>
{
	#data = new UmbObjectState<EntityType | undefined>(undefined);
	name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MemberType', new UmbMemberTypeRepository(host));
	}

	async load(entityId: string) {
		const { data } = await this.repository.requestById(entityId);
		if (data) {
			this.setIsNew(false);
			this.#data.next(data);
		}
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold();
		if (!data) return;
		this.setIsNew(true);
		this.#data.next(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.id || '';
	}

	getEntityType() {
		return 'member-type';
	}

	setName(name: string) {
		this.#data.update({ name });
	}

	setPropertyValue(alias: string, value: unknown) {
		// Not implemented
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.id) return;

		if (this.isNew) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value.id, this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	async delete(id: string) {
		await this.repository.delete(id);
	}

	public destroy(): void {
		this.#data.complete();
	}
}

export const UMB_MEMBER_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<UmbSaveableWorkspaceContextInterface, UmbMemberTypeWorkspaceContext>(
	'UmbWorkspaceContext',
	(context): context is UmbMemberTypeWorkspaceContext => context.getEntityType?.() === 'member-type'
);
