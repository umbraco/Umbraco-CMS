import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import { UMB_SECTION_CONTEXT } from '@umbraco-cms/backoffice/section';
import { UmbEntityExpansionManager, type UmbEntityExpansionModel } from '@umbraco-cms/backoffice/utils';

/**
 * Manages the expansion state of the section sidebar menu.
 * @exports
 * @class UmbSectionSidebarMenuExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbSectionSidebarMenuExpansionManager extends UmbControllerBase {
	#manager = new UmbEntityExpansionManager(this);
	public readonly expansion = this.#manager.expansion;

	#sectionContext?: typeof UMB_SECTION_CONTEXT.TYPE;
	#currentSectionAlias?: string;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_SECTION_CONTEXT, (sectionContext) => {
			this.#sectionContext = sectionContext;
			this.#observeCurrentSectionAlias();
		});
	}

	#observeCurrentSectionAlias() {
		this.observe(this.#sectionContext?.alias, (alias) => {
			if (!alias) return;
			this.#currentSectionAlias = alias;
			console.log(`SectionSidebarMenuExpansionManager: Current section alias set to: ${this.#currentSectionAlias}`);
		});
	}

	/**
	 * Checks if an entity is expanded
	 * @param {UmbEntityModel} entity The entity to check
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @returns {Observable<boolean>} True if the entity is expanded
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 */
	isExpanded(entity: UmbEntityModel): Observable<boolean> {
		return this.#manager.isExpanded(entity);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbEntityExpansionModel | undefined} expansion The expansion state
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbEntityExpansionModel): void {
		this.#manager.setExpansion(expansion);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 * @returns {UmbEntityExpansionModel} The expansion state
	 */
	getExpansion(): UmbEntityExpansionModel {
		return this.#manager.getExpansion();
	}

	/**
	 * Opens a menu item
	 * @param {UmbEntityModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entity: UmbEntityModel): Promise<void> {
		this.#manager.expandItem(entity);
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel} entities The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entities: UmbEntityExpansionModel): void {
		this.#manager.expandItems(entities);
	}

	/**
	 * Closes a menu item
	 * @param {UmbEntityModel} entity The entity to close
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entity: UmbEntityModel): Promise<void> {
		this.#manager.collapseItem(entity);
	}

	/**
	 * Closes all menu items
	 * @memberof UmbSectionSidebarMenuExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this.#manager.collapseAll();
	}
}
