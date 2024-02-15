import { UmbMemberTypeDetailRepository } from '../repository/detail/index.js';
import type { UmbMemberTypeDetailModel } from '../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export class UmbMemberTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberTypeDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository = new UmbMemberTypeDetailRepository(this);

	#data = new UmbObjectState<UmbMemberTypeDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MemberType');
	}

	async load(unique: string) {
		const { data } = await this.detailRepository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create(parentUnique: string | null) {
		const { data } = await this.detailRepository.createScaffold();

		if (data) {
			this.setIsNew(true);
			this.#data.setValue(data);
		}

		return { data };
	}

	async save() {
		const data = this.getData();
		if (!data) throw new Error('No data to save');

		if (this.getIsNew()) {
			await this.detailRepository.create(data);
		} else {
			await this.detailRepository.save(data);
		}

		this.saveComplete(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'member-type';
	}

	getName() {
		return this.#data.getValue()?.name;
	}

	setName(name: string | undefined) {
		this.#data.update({ name });
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
