import { UmbMemberGroupDetailRepository } from '../repository/index.js';
import type { UmbMemberGroupDetailModel } from '../types.js';
import { UMB_MEMBER_GROUP_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberGroupWorkspaceEditorElement } from './member-group-workspace-editor.element.js';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbSaveableWorkspaceContextBase,
	UmbWorkspaceRouteManager,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

export class UmbMemberGroupWorkspaceContext
	extends UmbSaveableWorkspaceContextBase<UmbMemberGroupDetailModel>
	implements UmbSaveableWorkspaceContextInterface, UmbRoutableWorkspaceContext
{
	public readonly repository = new UmbMemberGroupDetailRepository(this);
	#getDataPromise?: Promise<any>;

	#data = new UmbObjectState<UmbMemberGroupDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.unique);
	readonly name = this.#data.asObservablePart((data) => data?.name);

	readonly routes = new UmbWorkspaceRouteManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_GROUP_WORKSPACE_ALIAS);

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbMemberGroupWorkspaceEditorElement,
				setup: () => {
					this.create();

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbMemberGroupWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	public isLoaded() {
		return this.#getDataPromise;
	}

	protected resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbMemberGroupDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;

		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}

		this.observe(
			asObservable(),
			(memberGroup) => this.#onMemberGroupStoreChange(memberGroup),
			'umbMemberGroupStoreObserver',
		);
	}

	#onMemberGroupStoreChange(memberGroup: UmbMemberGroupDetailModel | undefined) {
		if (!memberGroup) {
			history.pushState(null, '', 'section/member-management/view/member-groups');
		}
	}

	async create() {
		this.resetState();
		this.#getDataPromise = this.repository.createScaffold();
		const { data } = await this.#getDataPromise;

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

		this.setIsNew(false);
		this.workspaceComplete(data);
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.unique;
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
		this.#data.destroy();
		super.destroy();
	}
}

export { UmbMemberGroupWorkspaceContext as api };
