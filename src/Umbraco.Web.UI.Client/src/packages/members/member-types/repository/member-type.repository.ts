import type { MemberTypeDetails } from '../types.js';
import { UmbMemberTypeTreeServerDataSource } from './sources/member-type.tree.server.data.js';
import { UmbMemberTypeTreeStore, UMB_MEMBER_TYPE_TREE_STORE_CONTEXT_TOKEN } from './member-type.tree.store.js';
import { UmbMemberTypeStore, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN } from './member-type.store.js';
import { UmbMemberTypeDetailServerDataSource } from './sources/member-type.detail.server.data.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbTreeDataSource, UmbDetailRepository, UmbTreeRepository } from '@umbraco-cms/backoffice/repository';
import { UmbNotificationContext, UMB_NOTIFICATION_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/notification';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

// TODO => use correct type when available
type ItemType = any;
type TreeItemType = EntityTreeItemResponseModel;

export class UmbMemberTypeRepository implements UmbTreeRepository<TreeItemType>, UmbDetailRepository<ItemType> {
	#init!: Promise<unknown>;

	#host: UmbControllerHostElement;

	#treeSource: UmbTreeDataSource;
	#treeStore?: UmbMemberTypeTreeStore;

	#detailSource: UmbMemberTypeDetailServerDataSource;
	#store?: UmbMemberTypeStore;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;

		// TODO: figure out how spin up get the correct data source
		this.#treeSource = new UmbMemberTypeTreeServerDataSource(this.#host);
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

	// TREE:
	async requestTreeRoot() {
		await this.#init;

		const data = {
			id: null,
			type: 'member-type-root',
			name: 'Member Types',
			icon: 'umb:folder',
			hasChildren: true,
		};

		return { data };
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#treeSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.rootItems };
	}

	async requestTreeItemsOf(parentId: string | null) {
		await this.#init;
		if (parentId === undefined) throw new Error('Parent id is missing');

		const { data, error } = await this.#treeSource.getChildrenOf(parentId);

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error, asObservable: () => this.#treeStore!.childrenOf(parentId) };
	}

	async requestItemsLegacy(ids: Array<string>) {
		await this.#init;

		if (!ids) {
			throw new Error('Ids are missing');
		}

		const { data, error } = await this.#treeSource.getItems(ids);

		return { data, error, asObservable: () => this.#treeStore!.items(ids) };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems;
	}

	async treeItemsOf(parentId: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentId);
	}

	async itemsLegacy(ids: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(ids);
	}

	// DETAILS

	async createScaffold() {
		await this.#init;
		return this.#detailSource.createScaffold();
	}

	async requestById(id: string) {
		await this.#init;

		// TODO: should we show a notification if the id is missing?
		// Investigate what is best for Acceptance testing, cause in that perspective a thrown error might be the best choice?
		if (!id) {
			throw new Error('Id is missing');
		}
		const { data, error } = await this.#detailSource.requestById(id);

		if (data) {
			this.#store?.append(data);
		}
		return { data, error };
	}
	async byId(id: string) {
		if (!id) throw new Error('Id is missing');
		await this.#init;
		return this.#store!.byId(id);
	}

	async delete(id: string) {
		await this.#init;

		if (!id) {
			throw new Error('Id is missing');
		}

		const { error } = await this.#detailSource.delete(id);

		if (!error) {
			const notification = { data: { message: `Member type deleted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		// TODO: we currently don't use the detail store for anything.
		// Consider to look up the data before fetching from the server.
		// Consider notify a workspace if a member type is deleted from the store while someone is editing it.
		this.#store?.remove([id]);
		this.#treeStore?.removeItem(id);
		// TODO: would be nice to align the stores on methods/methodNames.

		return { error };
	}

	async save(id: string, updatedMemberType: any) {
		if (!id) throw new Error('Key is missing');
		if (!updatedMemberType) throw new Error('Member Type is missing');

		await this.#init;

		const { error } = await this.#detailSource.update(id, updatedMemberType);

		if (!error) {
			// TODO: we currently don't use the detail store for anything.
			// Consider to look up the data before fetching from the server
			// Consider notify a workspace if a member type is updated in the store while someone is editing it.
			// TODO: would be nice to align the stores on methods/methodNames.
			//this.#store?.append(detail);
			this.#treeStore?.updateItem(id, updatedMemberType);

			const notification = { data: { message: `Member type '${updatedMemberType.name}' saved` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}

	async create(detail: MemberTypeDetails) {
		await this.#init;

		if (!detail.name) {
			throw new Error('Name is missing');
		}

		const { data, error } = await this.#detailSource.insert(detail);

		if (!error) {
			const notification = { data: { message: `Member type '${detail.name}' created` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { data, error };
	}
}
