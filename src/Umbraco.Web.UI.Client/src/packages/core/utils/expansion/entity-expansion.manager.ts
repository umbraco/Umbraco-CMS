import type { UmbEntityExpansionEntryModel, UmbEntityExpansionModel } from './types.js';
import {
	UmbExpansionEntityExpandedEvent,
	UmbExpansionEntityCollapsedEvent,
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
		this.getHostElement()?.dispatchEvent(new UmbExpansionEntityExpandedEvent(entity));
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
			this.getHostElement()?.dispatchEvent(new UmbExpansionEntityExpandedEvent(entity));
		});
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses an entity
	 * @param {EntryModelType} entity The entity to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entity: EntryModelType): Promise<void> {
		this._expansion.filter((x) => x.entityType !== entity.entityType || x.unique !== entity.unique);
		this.getHostElement()?.dispatchEvent(
			new UmbExpansionEntityCollapsedEvent({ entityType: entity.entityType, unique: entity.unique }),
		);
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses multiple entities
	 * @param {UmbEntityExpansionModel<EntryModelType>} entities The entities to close
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public collapseItems(entities: UmbEntityExpansionModel<EntryModelType>): void {
		if (!entities || entities.length === 0) return;
		this._expansion.filter(
			(x) => !entities.some((entity) => entity.entityType === x.entityType && entity.unique === x.unique),
		);
		entities.forEach((entity) => {
			this.getHostElement()?.dispatchEvent(
				new UmbExpansionEntityCollapsedEvent({ entityType: entity.entityType, unique: entity.unique }),
			);
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

	/**
	 * Gets an item from the expansion state
	 * @param {EntryModelType} entity The entity to get
	 * @returns {*}  {(Promise<UmbEntityExpansionEntryModel | undefined>)}
	 * @memberof UmbEntityExpansionManager
	 */
	public async getItem(entity: EntryModelType): Promise<EntryModelType | undefined> {
		const expansion = this._expansion.getValue();
		return expansion.find((x) => x.entityType === entity.entityType && x.unique === entity.unique);
	}
}
