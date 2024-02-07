import { UmbMemberTypeDetailRepository } from '../repository/detail/index.js';
import type { UmbMemberTypeDetailModel } from '../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbMemberTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberTypeDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	//
	public readonly repository = new UmbMemberTypeDetailRepository(this);

	#data = new UmbObjectState<UmbMemberTypeDetailModel | undefined>(undefined);
	name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.MemberType');
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async createScaffold() {
		const { data } = await this.repository.createScaffold(null);
		if (!data) return;
		this.setIsNew(true);
		this.#data.setValue(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.unique;
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
		if (!this.#data.value.unique) return;

		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value);
		}
		// If it went well, then its not new anymore?.
		this.setIsNew(false);
	}

	async delete(unique: string) {
		await this.repository.delete(unique);
	}

	public destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_MEMBER_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberTypeWorkspaceContext => context.getEntityType?.() === 'member-type',
);
