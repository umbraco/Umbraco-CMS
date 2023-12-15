import { UmbScriptTreeRepository } from './script-tree.repository.js';
import { UMB_ACTION_EVENT_CONTEXT, UmbActionEvent, UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';

/**
 * @export
 * @class UmbScriptTreeStore
 * @extends {UmbUniqueTreeStore}
 * @description - Tree Data Store for Scripts
 */
export class UmbScriptTreeStore extends UmbUniqueTreeStore {
	#actionEventContext?: UmbActionEventContext;
	#treeRepository: UmbScriptTreeRepository;

	/**
	 * Creates an instance of UmbScriptTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbScriptTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_SCRIPT_TREE_STORE_CONTEXT.toString());

		this.#treeRepository = new UmbScriptTreeRepository(host);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
			this.#listen();
		});
	}

	#listen() {
		this.#actionEventContext?.addEventListener('create-success', this.#onCreated);
		this.#actionEventContext?.addEventListener('save-success', this.#onSaved);
		this.#actionEventContext?.addEventListener('delete-success', this.#onDeleted);
	}

	#onCreated = (event: UmbActionEvent) => {
		// the item doesn't exist yet, so we reload the parent
		const eventParentUnique = event.getParentUnique();
		this.#treeRepository.requestTreeItemsOf(eventParentUnique);
	};

	#onSaved = (event: UmbActionEvent) => {
		// only reload the parent if the item is already in the store
		const eventUnique = event.getUnique();
		const storeItem = this.getItems([eventUnique])?.[0];
		if (storeItem) {
			this.#treeRepository.requestTreeItemsOf(storeItem.parentUnique);
		}
	};

	#onDeleted = (event: UmbActionEvent) => {
		this.removeItem(event.getUnique());
	};

	onDestroy() {
		this.#actionEventContext?.removeEventListener('create-success', this.#onCreated);
		this.#actionEventContext?.removeEventListener('save-success', this.#onSaved);
		this.#actionEventContext?.removeEventListener('delete-success', this.#onDeleted);
	}
}

export const UMB_SCRIPT_TREE_STORE_CONTEXT = new UmbContextToken<UmbScriptTreeStore>('UmbScriptTreeStore');
