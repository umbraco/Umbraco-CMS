import { UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT } from '../../global-context/section-sidebar-menu.global-context.token.js';
import type { UmbSectionMenuItemExpansionEntryModel } from '../../types.js';
import type { UmbMenuItemExpansionEntryModel } from '../../../components/menu/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbEntityExpansionManager, type UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

/**
 * Manages the expansion state of a section sidebar menu.
 * @exports
 * @class UmbSectionSidebarMenuSectionExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbSectionSidebarMenuSectionExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager<UmbMenuItemExpansionEntryModel>(this);
	public readonly expansion = this.#manager.expansion;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#globalContext?: typeof UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT.TYPE;
	#currentSectionAlias?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_SECTION_CONTEXT, (sectionContext) => {
			this.#sectionContext = sectionContext;
			this.#observeCurrentSectionAlias();
		});

		this.consumeContext(UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT, (globalContext) => {
			this.#globalContext = globalContext;
			this.#observeGlobalMenuExpansion();
		});
	}

	#observeCurrentSectionAlias() {
		this.observe(this.#sectionContext?.alias, (alias) => {
			if (!alias) return;
			this.#currentSectionAlias = alias;
		});
	}

	#observeGlobalMenuExpansion() {
		if (!this.#globalContext || !this.#currentSectionAlias) return;
		this.observe(this.#globalContext?.expansion.expansionBySectionAlias(this.#currentSectionAlias), (expansion) => {
			this.#manager.setExpansion(expansion);
		});
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
		const entries = this.#bindEntriesToSection(expansion);
		this.#globalContext?.expansion.setExpansion(entries);
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
		const entries = this.#bindEntriesToSection([entry]);
		this.#globalContext?.expansion.expandItem(entries[0]);
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>} entries The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entries: UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>): void {
		this.#manager.expandItems(entries);
		const entriesWithSectionAlias = this.#bindEntriesToSection(entries);
		this.#globalContext?.expansion.expandItems(entriesWithSectionAlias);
	}

	/**
	 * Closes a menu item
	 * @param {UmbMenuItemExpansionEntryModel} entry The item to close
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entry: UmbMenuItemExpansionEntryModel): Promise<void> {
		this.#manager.collapseItem(entry);
		const entries = this.#bindEntriesToSection([entry]);
		this.#globalContext?.expansion.collapseItem(entries[0]);
	}

	/**
	 * Closes all menu items
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		// Collapse all items in the global context matching the current section
		const entries = this.#bindEntriesToSection(this.#manager.getExpansion());
		this.#manager.collapseAll();
		this.#globalContext?.expansion.collapseItems(entries);
	}

	#bindEntriesToSection(
		expansion: UmbEntityExpansionModel<UmbMenuItemExpansionEntryModel>,
	): UmbEntityExpansionModel<UmbSectionMenuItemExpansionEntryModel> {
		return expansion.map((item) => ({
			...item,
			sectionAlias: this.#currentSectionAlias,
		}));
	}
}
