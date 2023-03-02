import { UmbMemberGroupTreeStore, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN } from './member-group.tree.store';
import { UmbMemberGroupDetailServerDataSource } from './sources/member-group.detail.server.data';
import { UmbMemberGroupStore, UMB_MEMBER_GROUP_STORE_CONTEXT_TOKEN } from './member-group.store';
import { MemberGroupTreeServerDataSource } from './sources/member-group.tree.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { ProblemDetailsModel } from '@umbraco-cms/backend-api';
import type { RepositoryTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/repository';

// TODO => Update type when backend updated
export class UmbMemberGroupRepository implements UmbTreeRepository, UmbDetailRepository<any> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;

	#treeSource: RepositoryTreeDataSource;
	#treeStore?: UmbMemberGroupTreeStore;

	#detailSource: UmbMemberGroupDetailServerDataSource;
	#store?: UmbMemberGroupStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostInterface) {
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

	async requestTreeItemsOf(parentKey: string | null) {
		const error: ProblemDetailsModel = { title: 'Not implemented' };
		return { data: undefined, error };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(keys);

		return { data, error };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentKey: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentKey);
	}

	async treeItems(keys: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(keys);
	}

	// DETAIL

	async createScaffold() {
		await this.#init;
		return this.#detailSource.createScaffold();
	}

	async requestByKey(key: string) {
		await this.#init;

		// TODO: should we show a notification if the key is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}
		const { data, error } = await this.#detailSource.get(key);

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

	async save(memberGroup: MemberGroupDetails) {
		await this.#init;

		if (!memberGroup || !memberGroup.name) {
			const error: ProblemDetailsModel = { title: 'Member group is missing' };
			return { error };
		}

		const { error } = await this.#detailSource.update(memberGroup);

		if (!error) {
			const notification = { data: { message: `Member group '${memberGroup.name} saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		this.#store?.append(memberGroup);
		this.#treeStore?.updateItem(memberGroup.key, { name: memberGroup.name });

		return { error };
	}

	async delete(key: string) {
		await this.#init;

		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		const { error } = await this.#detailSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Document deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a template is deleted from the store while someone is editing it.
		this.#store?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}
}
