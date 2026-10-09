import type { UmbTreeItemModel } from '../../types.js';
import { UmbTreeItemApiContextBase } from '../../tree-item-api/tree-item-api-context-base.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbElementControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';

class UmbTableTreeViewItemApi extends UmbTreeItemApiContextBase<UmbTreeItemModel> {}

export interface UmbTableTreeViewRowChangeCallbacks {
	onNoAccessChange: () => void;
	onPathChange: () => void;
	onActiveChange: () => void;
}

/**
 * Owns the per-row resources of a table tree view: the element-backed host that
 * provides the entity and tree-item-api contexts onto the row, the api itself,
 * and the observers that track the row's access, path and active state.
 *
 * Destroying the controller tears down everything it owns in a single call.
 */
export class UmbTableTreeViewRowController extends UmbControllerBase {
	#elementHost: UmbElementControllerHost;
	#entityContext: UmbEntityContext;
	#api: UmbTableTreeViewItemApi;

	#currentNoAccess = false;
	#currentPath = '';
	#currentIsActive = false;

	get currentNoAccess(): boolean {
		return this.#currentNoAccess;
	}
	get currentPath(): string {
		return this.#currentPath;
	}
	get currentIsActive(): boolean {
		return this.#currentIsActive;
	}

	constructor(
		element: HTMLElement,
		entityType: string | undefined,
		unique: string,
		treeItem: UmbTreeItemModel | undefined,
	) {
		const elementHost = new UmbElementControllerHost(element);
		super(elementHost);
		this.#elementHost = elementHost;
		this.#elementHost.hostConnected();

		this.#entityContext = new UmbEntityContext(this);
		this.#entityContext.setEntityType(entityType);
		this.#entityContext.setUnique(unique);

		this.#api = new UmbTableTreeViewItemApi(this);
		if (treeItem) this.#api.setTreeItem(treeItem);
	}

	/**
	 * Starts observing the api. Call this only after the controller has been
	 * registered by its owner, so the synchronous initial emissions can resolve
	 * the controller while updating the row.
	 * @param {UmbTableTreeViewRowChangeCallbacks} callbacks - invoked when the observed values change.
	 */
	observeApi(callbacks: UmbTableTreeViewRowChangeCallbacks): void {
		this.observe(
			this.#api.noAccess,
			(noAccess) => {
				this.#currentNoAccess = noAccess ?? false;
				callbacks.onNoAccessChange();
			},
			null,
		);

		this.observe(
			this.#api.path,
			(path) => {
				this.#currentPath = path ?? '';
				callbacks.onPathChange();
			},
			null,
		);

		this.observe(
			this.#api.isActive,
			(isActive) => {
				this.#currentIsActive = isActive ?? false;
				callbacks.onActiveChange();
			},
			null,
		);
	}

	setItem(entityType: string | undefined, unique: string, treeItem: UmbTreeItemModel | undefined): void {
		this.#entityContext.setEntityType(entityType);
		this.#entityContext.setUnique(unique);
		if (treeItem) this.#api.setTreeItem(treeItem);
	}

	override destroy(): void {
		super.destroy();
		this.#elementHost?.destroy();
		this.#elementHost = undefined as never;
	}
}
