import { MemberGroupTreeServerDataSource } from './sources/member-group.tree.server.data';
import { UmbMemberGroupTreeStore, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN } from './member-group.tree.store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbNotificationService, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/notification';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { ProblemDetails } from '@umbraco-cms/backend-api';
import type { UmbTreeRepository } from 'libs/repository/tree-repository.interface';
import type { MemberGroupDetails } from '@umbraco-cms/models';

export class UmbMemberGroupRepository implements UmbTreeRepository {
	#host: UmbControllerHostInterface;
	#dataSource: MemberGroupTreeServerDataSource;
	#treeStore?: UmbMemberGroupTreeStore;
	#notificationService?: UmbNotificationService;
	#initResolver?: () => void;
	#initialized = false;

	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
		// TODO: figure out how spin up get the correct data source
		this.#dataSource = new MemberGroupTreeServerDataSource(this.#host);

		new UmbContextConsumerController(this.#host, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#treeStore = instance;
			this.#checkIfInitialized();
		});

		new UmbContextConsumerController(this.#host, UMB_NOTIFICATION_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#notificationService = instance;
			this.#checkIfInitialized();
		});
	}

	#init = new Promise<void>((resolve) => {
		this.#initialized ? resolve() : (this.#initResolver = resolve);
	});

	#checkIfInitialized() {
		if (this.#treeStore && this.#notificationService) {
			this.#initialized = true;
			this.#initResolver?.();
		}
	}

	async requestRootTreeItems() {
		await this.#init;

		const { data, error } = await this.#dataSource.getRootItems();

		if (data) {
			this.#treeStore?.appendItems(data.items);
		}

		return { data, error };
	}

	async requestTreeItemsOf(parentKey: string | null) {
		const error: ProblemDetails = { title: 'Not implemented' };
		return { data: undefined, error };
	}

	async requestTreeItems(keys: Array<string>) {
		await this.#init;

		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { data: undefined, error };
		}

		const { data, error } = await this.#dataSource.getItems(keys);

		return { data, error };
	}

	async rootTreeItems() {
		await this.#init;
		return this.#treeStore!.rootItems();
	}

	async treeItemsOf(parentKey: string | null) {
		await this.#init;
		return this.#treeStore!.childrenOf(parentKey);
	}

	async treeItems(keys: Array<string>) {
		await this.#init;
		return this.#treeStore!.items(keys);
	}

	async saveDetail(memberGroup: MemberGroupDetails) {
		await this.#init;
		alert('implement save');
	}
}
