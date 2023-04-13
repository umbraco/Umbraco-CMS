import { UmbMemberGroupTreeStore, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN } from './member-group.tree.store';
import { UmbMemberGroupDetailServerDataSource } from './sources/member-group.detail.server.data';
import { UmbMemberGroupStore, UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN } from './member-group.store';
import { MemberGroupTreeServerDataSource } from './sources/member-group.tree.server.data';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import type { MemberGroupDetails } from '@umbraco-cms/backoffice/models';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';

// TODO => Update type when backend updated
export class UmbMemberGroupRepository implements UmbTreeRepository, UmbDetailRepository<any, any, any> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbMemberGroupTreeStore;

	#detailSource: UmbMemberGroupDetailServerDataSource;
	#store?: UmbMemberGroupStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new MemberGroupTreeServerDataSource(this.#host);
		this.#detailSource = new UmbMemberGroupDetailServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
		});

		new UmbContextConsumerController(this.#host, UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN, (instance) => {
			this.#store = instance;
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
			this.#notificationContext = instance;
		});
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestTreeItemsOf(parentId: string | null) {
		const error: ProblemDetailsModel = { title: 'Not implemented' };
		return { data: undefined, error };
	}

	async requestTreeItems(ids: Array<string>) {
		await this.#init;

		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(ids);

		return { data, error };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentId);
	}

	async treeItems(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAIL

	async createScaffold() {
		await this.#init;
		return this.#detailSource.createScaffold();
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Id is missing' };
			return { error };
		}
		const { data, error } = await this.#detailSource.get(id);

		if (data) {
			this.#store?.append(data);
		}
		return { data, error };
	}

	async create(detail: MemberGroupDetails) {
		await this.#init;

		if (!detail.name) {
			const error: ProblemDetailsModel = { title: 'Name is missing' };
			return { error };
		}

		const { data, error } = await this.#detailSource.insert(detail);

		if (!error) {
			const notification = { data: { message: `Member group '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}

	async save(id: string, memberGroup: MemberGroupDetails) {
		if (!id) throw new Error('Id is missing');
		if (!memberGroup) throw new Error('Member group is missing');

		await this.#init;

		const { error } = await this.#detailSource.update(id, memberGroup);

		if (!error) {
			this.#store?.append(memberGroup);
			this.#treeStore?.updateItem(memberGroup.id, memberGroup);

			const notification = { data: { message: `Member group '${memberGroup.name} saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async delete(id: string) {
		if (!id) throw new Error('Id is missing');

		await this.#init;

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server.
			// Consider notify a workspace if a template is deleted from the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			this.#store?.remove([id]);
			this.#treeStore?.removeItem(id);

			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}
