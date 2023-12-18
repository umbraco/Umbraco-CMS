import { UmbStylesheetTreeRepository } from './stylesheet-tree.repository.js';
import { UmbUniqueTreeStore } from '@umbraco-cms/backoffice/tree';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT, UmbActionEvent, UmbActionEventContext } from '@umbraco-cms/backoffice/action';

/**
 * @export
 * @class UmbStylesheetTreeStore
 * @extends {UmbEntityTreeStore}
 * @description - Tree Data Store for Stylesheets
 */
export class UmbStylesheetTreeStore extends UmbUniqueTreeStore {
	#actionEventContext?: UmbActionEventContext;
	#treeRepository: UmbStylesheetTreeRepository;

	/**
	 * Creates an instance of UmbStylesheetTreeStore.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbStylesheetTreeStore
	 */
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_STYLESHEET_TREE_STORE_CONTEXT.toString());

		this.#treeRepository = new UmbStylesheetTreeRepository(host);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
			this.#listen();
		});
	}

	#listen() {
		// TODO: add event class to remove the magic strings
		this.#actionEventContext?.addEventListener('detail-create-success', this.#onCreated);
		this.#actionEventContext?.addEventListener('detail-save-success', this.#onSaved);
		this.#actionEventContext?.addEventListener('detail-delete-success', this.#onDeleted);
	}

	#stopListening() {
		this.#actionEventContext?.removeEventListener('detail-create-success', this.#onCreated);
		this.#actionEventContext?.removeEventListener('detail-save-success', this.#onSaved);
		this.#actionEventContext?.removeEventListener('detail-delete-success', this.#onDeleted);
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

export const UMB_STYLESHEET_TREE_STORE_CONTEXT = new UmbContextToken<UmbStylesheetTreeStore>('UmbStylesheetTreeStore');
