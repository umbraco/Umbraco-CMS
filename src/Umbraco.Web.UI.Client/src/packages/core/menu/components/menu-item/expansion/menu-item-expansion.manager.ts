import type { UmbMenuItemExpansionEntryModel } from '../../menu/types.js';
import { UMB_MENU_CONTEXT } from '../../menu/constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	createObservablePart,
	type UmbObserverController,
	type Observable,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbEntityExpansionManager, type UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

/**
 * Manages the expansion state of a menu item
 * @exports
 * @class UmbMenuItemExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbMenuItemExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager<UmbMenuItemExpansionEntryModel>(this);
	public readonly expansion = this.#manager.expansion;

	#menuItemAlias?: string;
	#menuContext?: typeof UMB_MENU_CONTEXT.TYPE;
	#observerController?: UmbObserverController;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_MENU_CONTEXT, (menuContext) => {
			this.#menuContext = menuContext;
			this.#observeMenuExpansion();
		});
	}

	#observeMenuExpansion() {
		if (!this.#menuContext || !this.#menuItemAlias) {
			this.#observerController?.destroy();
			return;
		}

		this.#observerController = this.observe(
			createObservablePart(
				this.#menuContext.expansion.expansion,
				(items: Array<UmbMenuItemExpansionEntryModel>) =>
					items?.filter((item) => item.menuItemAlias === this.#menuItemAlias) || [],
			),
			(itemsForMenuItem) => {
				this.#manager.setExpansion(itemsForMenuItem);
			},
			'observeMenuExpension',
		);
	}

	setMenuItemAlias(alias: string | undefined): void {
		this.#menuItemAlias = alias;
		this.#observeMenuExpansion();
	}

	/**
	 * Checks if an entry is expanded
	 * @param {UmbMenuItemExpansionEntryModel} entry The entry to check
	 * @returns {Observable<boolean>} True if the entry is expanded
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 */
	isExpanded(entry: UmbMenuItemExpansionEntryModel): Observable<boolean> {
		return this.#manager.isExpanded(entry);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel> | undefined} expansion The expansion state
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>): void {
		this.#manager.setExpansion(expansion);
		this.#menuContext?.expansion.setExpansion(expansion);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>} The expansion state
	 */
	getExpansion(): UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel> {
		return this.#manager.getExpansion();
	}

	/**
	 * Opens a menu item
	 * @param {UmbMenuItemExpansionEntryModel} entry The item to open
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entry: UmbMenuItemExpansionEntryModel): Promise<void> {
		this.#manager.expandItem(entry);
		this.#menuContext?.expansion.expandItem(entry);
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>} entries The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entries: UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>): void {
		this.#manager.expandItems(entries);
		this.#menuContext?.expansion.expandItems(entries);
	}

	/**
	 * Closes a menu item
	 * @param {UmbMenuItemExpansionEntryModel} entry The item to close
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entry: UmbMenuItemExpansionEntryModel): Promise<void> {
		this.#manager.collapseItem(entry);
		this.#menuContext?.expansion.collapseItem(entry);
	}

	/**
	 * Closes all menu items
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		const localEntries = this.#manager.getExpansion();
		this.#manager.collapseAll();
		this.#menuContext?.expansion.collapseItems(localEntries);
	}
}
