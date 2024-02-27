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

type EntityType = UmbMemberDetailModel;
export class UmbMemberWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbMemberDetailRepository(this);

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);

	readonly email = this.#currentData.asObservablePart((data) => data?.email);
	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_WORKSPACE_ALIAS);
	}

	resetState() {
		super.resetState();
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
		}
	}

	async create(parentUnique: string | null, memberTypeUnique: string) {
		this.resetState();
		const { data } = await this.repository.createScaffold(parentUnique, {
			memberType: { unique: memberTypeUnique },
		});

		if (data) {
			this.setIsNew(true);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
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
	updateData(data: Partial<EntityType>) {
		const currentData = this.#currentData.getValue();
		if (!currentData) throw new Error('No data to update');
		this.#currentData.setValue({ ...currentData, ...data });
	}

	getData() {
		return this.#currentData.getValue();
	}

	getUnique() {
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
