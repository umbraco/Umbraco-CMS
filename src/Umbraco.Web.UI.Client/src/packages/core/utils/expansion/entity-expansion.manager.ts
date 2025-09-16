import type { UmbEntityExpansionEntryModel, UmbEntityExpansionModel } from './types.js';
import {
	UmbExpansionEntryExpandedEvent,
	UmbExpansionEntryCollapsedEvent,
	UmbExpansionChangeEvent,
} from './events/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages an expansion state
 * @exports
 * @class UmbEntityExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbEntityExpansionManager<
	EntryModelType extends UmbEntityExpansionEntryModel = UmbEntityExpansionEntryModel,
> extends UmbControllerBase {
	protected _expansion = new UmbArrayState<EntryModelType>([], (x) => x.entityType + x.unique);
	expansion = this._expansion.asObservable();

	/**
	 * Checks if an entity is expanded
	 * @param {EntryModelType} entity The entity to check
	 * @returns {Observable<boolean>} True if the entity is expanded
	 * @memberof UmbEntityExpansionManager
	 */
	isExpanded(entity: EntryModelType): Observable<boolean> {
		return this._expansion.asObservablePart((entries) =>
			entries?.some((entry) => entry.entityType === entity.entityType && entry.unique === entity.unique),
		);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbEntityExpansionModel<EntryModelType> | undefined} expansion The expansion state
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbEntityExpansionModel<EntryModelType>): void {
		this._expansion.setValue(expansion);
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbEntityExpansionManager
	 * @returns {UmbEntityExpansionModel} The expansion state
	 */
	getExpansion(): UmbEntityExpansionModel<EntryModelType> {
		return this._expansion.getValue();
	}

	/**
	 * Expands an entity
	 * @param {EntryModelType} entity The entity to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entity: EntryModelType): Promise<void> {
		this._expansion.appendOne(entity);
		this.getHostElement()?.dispatchEvent(new UmbExpansionEntryExpandedEvent(entity));
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel<EntryModelType>} entities The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entities: UmbEntityExpansionModel<EntryModelType>): void {
		if (!entities || entities.length === 0) return;
		this._expansion.append(entities);
		entities.forEach((entity) => {
			this.getHostElement()?.dispatchEvent(new UmbExpansionEntryExpandedEvent(entity));
		});
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses an entity
	 * @param {EntryModelType} entry The entity to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entry: EntryModelType): Promise<void> {
		this._expansion.filter((x) => x.entityType !== entry.entityType || x.unique !== entry.unique);
		this.getHostElement()?.dispatchEvent(new UmbExpansionEntryCollapsedEvent(entry));
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses multiple entities
	 * @param {UmbEntityExpansionModel<EntryModelType>} entries The entities to close
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public collapseItems(entries: UmbEntityExpansionModel<EntryModelType>): void {
		if (!entries || entries.length === 0) return;
		this._expansion.filter(
			(x) => !entries.some((entry) => entry.entityType === x.entityType && entry.unique === x.unique),
		);
		entries.forEach((entry) => {
			this.getHostElement()?.dispatchEvent(new UmbExpansionEntryCollapsedEvent(entry));
		});
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses all expanded entities
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this._expansion.setValue([]);
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}
}
