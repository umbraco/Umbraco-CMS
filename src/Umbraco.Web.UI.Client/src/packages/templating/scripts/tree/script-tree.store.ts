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
		// TODO: add event class to remove the magic strings
		this.#actionEventContext?.addEventListener('detail-create-success', this.#onCreated as EventListener);
		this.#actionEventContext?.addEventListener('detail-save-success', this.#onSaved as EventListener);
		this.#actionEventContext?.addEventListener('detail-delete-success', this.#onDeleted as EventListener);
	}

	#stopListening() {
		this.#actionEventContext?.removeEventListener('detail-create-success', this.#onCreated as EventListener);
		this.#actionEventContext?.removeEventListener('detail-save-success', this.#onSaved as EventListener);
		this.#actionEventContext?.removeEventListener('detail-delete-success', this.#onDeleted as EventListener);
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

		/* we need to remove the store because the unique (path) can have changed.
		and it will therefore not update the correct item but append a new. */
		if (storeItem) {
			this.removeItem(eventUnique);
			this.#treeRepository.requestTreeItemsOf(storeItem.parentUnique);
		}
	};

	#onDeleted = (event: UmbActionEvent) => {
		this.removeItem(event.getUnique());
	};

	onDestroy() {
		this.#stopListening();
	}
}

export const UMB_SCRIPT_TREE_STORE_CONTEXT = new UmbContextToken<UmbScriptTreeStore>('UmbScriptTreeStore');
