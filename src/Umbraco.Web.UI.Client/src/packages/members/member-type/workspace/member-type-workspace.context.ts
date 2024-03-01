import { UmbMemberTypeDetailRepository } from '../repository/detail/index.js';
import type { UmbMemberTypeDetailModel } from '../types.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from '@umbraco-cms/backoffice/tree';

export class UmbMemberTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbMemberTypeDetailModel>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly detailRepository = new UmbMemberTypeDetailRepository(this);

	#parent?: { entityType: string; unique: string | null };

	#data = new UmbObjectState<UmbMemberTypeDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart((data) => data?.name);

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.MemberType');
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

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent = parent;
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
			if (!this.#parent) throw new Error('Parent is not set');
			await this.detailRepository.create(data, this.#parent.unique);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbReloadTreeItemChildrenRequestEntityActionEvent({
				entityType: this.#parent.entityType,
				unique: this.#parent.unique,
			});
			eventContext.dispatchEvent(event);
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
