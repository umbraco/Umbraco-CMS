import { createFolderTreeItem } from '../utils.js';
import { UMB_DATA_TYPE_TREE_STORE_CONTEXT, UmbDataTypeTreeStore } from '../../tree/data-type-tree.store.js';
import { UmbDataTypeFolderServerDataSource } from './data-type-folder.server.data.js';
import {
	type UmbFolderRepository,
	type UmbFolderDataSource,
	UmbRepositoryBase,
} from '@umbraco-cms/backoffice/repository';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { CreateFolderRequestModel, FolderModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_NOTIFICATION_CONTEXT_TOKEN, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
export class UmbDataTypeFolderRepository extends UmbRepositoryBase implements UmbFolderRepository {
	#init: Promise<unknown>;
	#folderSource: UmbFolderDataSource;
	#treeStore?: UmbDataTypeTreeStore;
	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#folderSource = new UmbDataTypeFolderServerDataSource(this);

		this.#init = Promise.all([
			this.consumeContext(UMB_DATA_TYPE_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}).asPromise(),

			this.consumeContext(UMB_NOTIFICATION_CONTEXT_TOKEN, (instance) => {
				this.#notificationContext = instance;
			}).asPromise(),
		]);
	}

	async createFolderScaffold(parentId: string | null) {
		if (parentId === undefined) throw new Error('Parent id is missing');
		await this.#init;
		return this.#folderSource.createScaffold(parentId);
	}

	// TODO: temp create type until backend is ready. Remove the id addition when new types are generated.
	async createFolder(folderRequest: CreateFolderRequestModel) {
		if (!folderRequest) throw new Error('folder request is missing');
		await this.#init;

		const { error } = await this.#folderSource.create(folderRequest);

		if (!error) {
			// TODO: We need to push a new item to the tree store to update the tree. How do we want to create the tree items?
			const folderTreeItem = createFolderTreeItem(folderRequest);
			this.#treeStore!.append(folderTreeItem);
		}

		return { error };
	}

	async deleteFolder(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.#init;

		const { error } = await this.#folderSource.delete(id);

		if (!error) {
			this.#treeStore!.removeItem(id);
		}

		return { error };
	}

	async updateFolder(id: string, folder: FolderModelBaseModel) {
		if (!id) throw new Error('Key is missing');
		if (!folder) throw new Error('Folder data is missing');
		await this.#init;

		const { error } = await this.#folderSource.update(id, folder);

		if (!error) {
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.#treeStore!.updateItem(id, { name: folder.name });
		}

		return { error };
	}

	async requestFolder(id: string) {
		if (!id) throw new Error('Key is missing');
		await this.#init;
		return await this.#folderSource.read(id);
	}
}
