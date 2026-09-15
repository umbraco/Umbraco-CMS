import { UmbDocumentCurrentVariantResolver } from '../variant/index.js';
import type { UmbDocumentVariantState } from '../variant-state.js';
import type { UmbDocumentTreeItemModel, UmbDocumentTreeItemVariantModel } from './types.js';
import { UmbBasicState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

/**
 * A controller for resolving data for a document tree item
 * @exports
 * @class UmbDocumentTreeItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentTreeItemDataResolver
	extends UmbControllerBase
	implements UmbItemDataResolver<UmbDocumentTreeItemModel>
{
	#data = new UmbObjectState<UmbDocumentTreeItemModel | undefined>(undefined);
	#variant = new UmbDocumentCurrentVariantResolver<UmbDocumentTreeItemVariantModel>(this);

	public readonly entityType = this.#data.asObservablePart((x) => x?.entityType);
	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.documentType.icon);
	public readonly typeUnique = this.#data.asObservablePart((x) => x?.documentType.unique);
	public readonly isTrashed = this.#data.asObservablePart((x) => x?.isTrashed);
	public readonly hasCollection = this.#data.asObservablePart((x) => !!x?.documentType.collection);

	public readonly name = this.#variant.name;
	public readonly state = this.#variant.state;
	public readonly isDraft = this.#variant.isDraft;
	public readonly flags = this.#variant.flags;

	// A tree item carries its create date on the item itself, as a string, and carries no update date.
	public readonly createDate = this.#data.asObservablePart((x) => (x?.createDate ? new Date(x.createDate) : undefined));
	public readonly updateDate = new UmbBasicState<Date | undefined>(undefined).asObservable();

	/**
	 * Get the display culture or fallback culture
	 * @returns {string | null | undefined} The display culture or fallback culture
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	getCulture(): string | null | undefined {
		return this.#variant.getCulture();
	}

	/**
	 * Get the current tree item
	 * @returns {UmbDocumentTreeItemModel | undefined} The current tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	getData(): UmbDocumentTreeItemModel | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current tree item
	 * @param {UmbDocumentTreeItemModel | undefined} data The current tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	setData(data: UmbDocumentTreeItemModel | undefined) {
		this.#data.setValue(data);
		this.#variant.setVariants(data?.variants, data?.flags);
	}

	/**
	 * Get the entity type of the tree item
	 * @returns {Promise<string | undefined>} The entity type of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	/**
	 * Get the unique of the tree item
	 * @returns {Promise<string | undefined>} The unique of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	/**
	 * Get the name of the tree item
	 * @returns {Promise<string>} The name of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getName(): Promise<string> {
		return (await this.observe(this.name).asPromise()) || '';
	}

	/**
	 * Get the icon of the tree item
	 * @returns {Promise<string | undefined>} The icon of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}

	/**
	 * Get the state of the tree item
	 * @returns {Promise<string | undefined>} The state of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getState(): Promise<UmbDocumentVariantState | null | undefined> {
		return await this.observe(this.state).asPromise();
	}

	/**
	 * Get the isDraft of the tree item
	 * @returns {Promise<boolean>} The isDraft of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getIsDraft(): Promise<boolean> {
		return (await this.observe(this.isDraft).asPromise()) ?? false;
	}

	/**
	 * Get the isTrashed of the tree item
	 * @returns {Promise<boolean>} The isTrashed of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getIsTrashed(): Promise<boolean> {
		return (await this.observe(this.isTrashed).asPromise()) ?? false;
	}

	/**
	 * Get the create date of the tree item
	 * @returns {Promise<Date>} The create date of the tree item
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	async getCreateDate(): Promise<Date> {
		return (await this.observe(this.createDate).asPromise()) || undefined;
	}

	/**
	 * Test if the tree item has a collection
	 * @returns {boolean} Boolean of whether the tree item has a collection.
	 * @memberof UmbDocumentTreeItemDataResolver
	 */
	getHasCollection(): boolean {
		return this.getData()?.documentType.collection != undefined;
	}
}
