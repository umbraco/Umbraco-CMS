import { UMB_SECTION_SIDEBAR_MENU_GLOBAL_CONTEXT } from '../../global-context/section-sidebar-menu.global-context.token.js';
import type { UmbEntityExpansionSectionEntryModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import {
	UmbEntityExpansionManager,
	type UmbEntityExpansionEntryModel,
	type UmbEntityExpansionModel,
} from '@umbraco-cms/backoffice/utils';

/**
 * Manages the expansion state of a section sidebar menu.
 * @exports
 * @class UmbSectionSidebarMenuSectionExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbSectionSidebarMenuSectionExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager(this);
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
	 * Checks if an entity is expanded
	 * @param {UmbEntityModel} entity The entity to check
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @returns {Observable<boolean>} True if the entity is expanded
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 */
	isExpanded(entity: UmbEntityModel): Observable<boolean> {
		return this.#manager.isExpanded(entity);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbEntityExpansionModel | undefined} expansion The expansion state
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbEntityExpansionModel): void {
		this.#manager.setExpansion(expansion);
		const entries = this.#bindEntriesToSection(expansion);
		this.#globalContext?.expansion.setExpansion(entries);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {UmbEntityExpansionModel} The expansion state
	 */
	getExpansion(): UmbEntityExpansionModel {
		return this.#manager.getExpansion();
	}

	/**
	 * Opens a menu item
	 * @param {UmbEntityExpansionEntryModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @param {UmbEntityModel} entity.target The target entity to look up
	 * @param {string} entity.target.entityType The target entity type
	 * @param {string} entity.target.unique The target unique key
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entity: UmbEntityExpansionEntryModel): Promise<void> {
		this.#manager.expandItem(entity);
		const entries = this.#bindEntriesToSection([entity]);
		this.#globalContext?.expansion.expandItem(entries[0]);
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel} entities The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entities: UmbEntityExpansionModel): void {
		this.#manager.expandItems(entities);
		const entries = this.#bindEntriesToSection(entities);
		this.#globalContext?.expansion.expandItems(entries);
	}

	/**
	 * Closes a menu item
	 * @param {UmbEntityExpansionEntryModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entity: UmbEntityModel): Promise<void> {
		this.#manager.collapseItem(entity);
		const entries = this.#bindEntriesToSection([entity]);
		this.#globalContext?.expansion.collapseItem(entries[0]);
	}

	/**
	 * Closes all menu items
	 * @memberof UmbSectionSidebarMenuSectionExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this.#manager.collapseAll();
		// Collapse all items in the global context matching the current section
		const entries = this.#bindEntriesToSection(this.#manager.getExpansion());
		this.#globalContext?.expansion.collapseItems(entries);
	}

	#bindEntriesToSection(expansion: UmbEntityExpansionModel): Array<UmbEntityExpansionSectionEntryModel> {
		return expansion.map((item) => ({
			...item,
			sectionAlias: this.#currentSectionAlias,
		}));
	}
}
