import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRepository,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbEntityDataPickerTreeRepository extends UmbRepositoryBase implements UmbTreeRepository, UmbApi {
	#treeRepository?: UmbTreeRepository;

	#init: Promise<[any]>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext('testy', (instance) => {
				this.#treeRepository = instance?.tree;
			}).asPromise(),
		]);
	}

	async requestTreeRoot() {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository.requestTreeRoot();
	}

	async requestTreeRootItems(args: UmbTreeRootItemsRequestArgs) {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository.requestTreeRootItems(args);
	}

	async requestTreeItemsOf(args: UmbTreeChildrenOfRequestArgs) {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository.requestTreeItemsOf(args);
	}

	async requestTreeItemAncestors(args: UmbTreeAncestorsOfRequestArgs) {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository.requestTreeItemAncestors(args);
	}

	/**
	 * @deprecated Use `requestTreeItemsOf` instead. It will be removed in Umbraco 18.
	 * @returns {Promise<Observable<UmbTreeItemModel[]>>} An observable of tree items.
	 */
	async rootTreeItems() {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository?.rootTreeItems();
	}

	/**
	 * @deprecated Use `requestTreeItemsOf` instead. It will be removed in Umbraco 18.
	 * @returns {Promise<Observable<UmbTreeItemModel[]>>} An observable of tree items.
	 */
	async treeItemsOf() {
		await this.#init;
		if (!this.#treeRepository) throw new Error('No tree repository set');
		return this.#treeRepository?.treeItemsOf();
	}

	override destroy(): void {
		this.#treeRepository = undefined;
		super.destroy();
	}
}

export { UmbEntityDataPickerTreeRepository as api };
