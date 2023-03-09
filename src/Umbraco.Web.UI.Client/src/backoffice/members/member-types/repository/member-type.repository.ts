import { MemberTypeTreeServerDataSource } from './sources/member-type.tree.server.data';
import { UmbMemberTypeTreeStore, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN } from './member-type.tree.store';
import { UmbMemberTypeStore, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN } from './member-type.store';
import { UmbMemberTypeDetailServerDataSource } from './sources/member-type.detail.server.data';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { RepositoryTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/repository';
import { ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import type { MemberTypeDetails } from '@umbraco-cms/models';

// TODO => use correct type when available
type ItemType = any;

export class UmbMemberTypeRepository implements UmbTreeRepository, UmbDetailRepository<ItemType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostInterface;

	#treeSource: RepositoryTreeDataSource;
	#treeStore?: UmbMemberTypeTreeStore;

	#detailSource: UmbMemberTypeDetailServerDataSource;
	#store?: UmbMemberTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new MemberTypeTreeServerDataSource(this.#host);
		this.#detailSource = new UmbMemberTypeDetailServerDataSource(this.#host);

		this.#init = Promise.all([
			new UmbContextConsumerController(this.#host, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#store = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN, (instance) => {
				this.#treeStore = instance;
			}),

			new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}),
		]);
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentKey: string | null) {
		await this.#init;

		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getChildrenOf(parentKey);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentKey) };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#treeSource.getItems(keys);

		return { data, error, asObservable: () => this.#treeStore!.items(keys) };
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

	// DETAILS

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
		const { data, error } = await this.#detailSource.requestByKey(key);

		if (data) {
			this.#store?.append(data);
		}
		return { data, error };
	}

	async delete(key: string) {
		await this.#init;

		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		const { error } = await this.#detailSource.delete(key);

		if (!error) {
			const notification = { data: { message: `Member type deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a member type is deleted from the store while someone is editing it.
		this.#store?.remove([key]);
		this.#treeStore?.removeItem(key);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async save(detail: ItemType) {
		await this.#init;

		// TODO: should we show a notification if the MemberType is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!detail || !detail.key) {
			const error: ProblemDetailsModel = { title: 'Member type is missing' };
			return { error };
		}

		const { error } = await this.#detailSource.save(detail);

		if (!error) {
			const notification = { data: { message: `Member type '${detail.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server
		// Consider notify a workspace if a member type is updated in the store while someone is editing it.
		this.#store?.append(detail);
		this.#treeStore?.updateItem(detail.key, { name: detail.name });
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async create(detail: MemberTypeDetails) {
		await this.#init;

		if (!detail.name) {
			const error: ProblemDetailsModel = { title: 'Name is missing' };
			return { error };
		}

		const { data, error } = await this.#detailSource.create(detail);

		if (!error) {
			const notification = { data: { message: `Member type '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
