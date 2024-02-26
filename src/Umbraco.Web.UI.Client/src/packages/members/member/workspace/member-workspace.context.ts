import { UmbMemberDetailRepository } from '../repository/index.js';
import type { UmbMemberDetailModel } from '../types.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbMemberWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbMemberDetailRepository(this);

	#data = new UmbObjectState<UmbMemberDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_WORKSPACE_ALIAS);
	}

	async load(unique: string) {
		const { data } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.setValue(data);
		}
	}

	async create(parentUnique: string | null, memberTypeUnique: string) {
		const { data } = await this.repository.createScaffold(parentUnique, {
			memberType: { unique: memberTypeUnique },
		});

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
			await this.repository.create(data);
		} else {
			await this.repository.save(data);
		}

		this.saveComplete(data);
	}

	// Only for CRUD demonstration purposes
	updateData(data: Partial<UmbMemberDetailModel>) {
		const currentData = this.#data.getValue();
		if (!currentData) throw new Error('No data to update');
		this.#data.setValue({ ...currentData, ...data });
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'member';
	}

	public destroy(): void {
		super.destroy();
	}
}

export const UMB_MEMBER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberWorkspaceContext => context.getEntityType?.() === 'member',
);
