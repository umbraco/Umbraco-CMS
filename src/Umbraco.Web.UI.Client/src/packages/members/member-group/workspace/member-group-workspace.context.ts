import { UmbMemberGroupDetailRepository } from '../repository/index.js';
import type { UmbMemberGroupDetailModel } from '../types.js';
import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from './manifests.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbMemberGroupWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberGroupDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository = new UmbMemberGroupDetailRepository(this);

	#data = new UmbObjectState<UmbMemberGroupDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_GROUP_WORKSPACE_ALIAS);
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.detailRepository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create(parentUnique: string | null) {
		this.resetState();
		const { data } = await this.detailRepository.createScaffold(parentUnique);

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

		this.setIsNew(false);
		this.workspaceComplete(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'member-group';
	}

	getName() {
		return this.#data.getValue()?.name;
	}

	setName(name: string | undefined) {
		this.#data.update({ name });
	}

	public destroy(): void {
		console.log('destroy');
	}
}

export const UMB_MEMBER_GROUP_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberGroupWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberGroupWorkspaceContext => context.getEntityType?.() === 'member-group',
);
