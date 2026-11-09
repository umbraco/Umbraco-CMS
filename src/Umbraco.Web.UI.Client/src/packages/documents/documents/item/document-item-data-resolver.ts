import type { UmbDocumentVariantState } from '../variant-state.js';
import { UmbDocumentCurrentVariantResolver } from '../variant/index.js';
import type { UmbDocumentItemModel, UmbDocumentItemVariantModel } from './types.js';
import { UmbBasicState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemDataResolver } from '@umbraco-cms/backoffice/entity-item';

// TODO: Take UmbDocumentItemModel directly and drop the generic. Both this alias and the generic only
// exist so models that are not document items could be passed in. Breaking, so needs a major. [MR]
type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver<DocumentItemModel extends UmbDocumentItemDataResolverModel>
	extends UmbControllerBase
	implements UmbItemDataResolver
{
	#data = new UmbObjectState<DocumentItemModel | undefined>(undefined);
	#variant = new UmbDocumentCurrentVariantResolver<UmbDocumentItemVariantModel>(this);

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

	// A document item carries its dates on the variant, so they follow whichever variant is resolved.
	#createDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly createDate = this.#createDate.asObservable();

	#updateDate = new UmbBasicState<Date | undefined>(undefined);
	public readonly updateDate = this.#updateDate.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.observe(
			this.#variant.currentVariant,
			(variant) => {
				this.#createDate.setValue(variant?.createDate);
				this.#updateDate.setValue(variant?.updateDate);
			},
			null,
		);
	}

	/**
	 * Get the display culture or fallback culture
	 * @returns {string | null | undefined} The display culture or fallback culture
	 * @memberof UmbDocumentItemDataResolver
	 */
	getCulture(): string | null | undefined {
		return this.#variant.getCulture();
	}

	/**
	 * Get the current item
	 * @returns {DocumentItemModel | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getData(): DocumentItemModel | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {DocumentItemModel | undefined} data The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setData(data: DocumentItemModel | undefined) {
		this.#data.setValue(data);
		this.#variant.setVariants(data?.variants, data?.flags);
	}

	/**
	 * Get the entity type of the item
	 * @returns {Promise<string | undefined>} The entity type of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getEntityType(): Promise<string | undefined> {
		return await this.observe(this.entityType).asPromise();
	}

	/**
	 * Get the unique of the item
	 * @returns {Promise<string | undefined>} The unique of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUnique(): Promise<string | undefined> {
		return await this.observe(this.unique).asPromise();
	}

	/**
	 * Get the name of the item
	 * @returns {Promise<string>} The name of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getName(): Promise<string> {
		return (await this.observe(this.name).asPromise()) || '';
	}

	/**
	 * Get the icon of the item
	 * @returns {Promise<string | undefined>} The icon of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIcon(): Promise<string | undefined> {
		return await this.observe(this.icon).asPromise();
	}

	/**
	 * Get the state of the item
	 * @returns {Promise<string | undefined>} The state of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getState(): Promise<UmbDocumentVariantState | null | undefined> {
		return await this.observe(this.state).asPromise();
	}

	/**
	 * Get the isDraft of the item
	 * @returns {Promise<boolean>} The isDraft of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsDraft(): Promise<boolean> {
		return (await this.observe(this.isDraft).asPromise()) ?? false;
	}

	/**
	 * Get the isTrashed of the item
	 * @returns {Promise<boolean | undefined>} The isTrashed of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getIsTrashed(): Promise<boolean> {
		return (await this.observe(this.isTrashed).asPromise()) ?? false;
	}

	/**
	 * Get the create date of the item
	 * @returns {Promise<Date>} The create date of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getCreateDate(): Promise<Date> {
		return (await this.observe(this.createDate).asPromise()) || undefined;
	}

	/**
	 * Get the update date of the item
	 * @returns {Promise<Date>} The update date of the item
	 * @memberof UmbDocumentItemDataResolver
	 */
	async getUpdateDate(): Promise<Date> {
		return (await this.observe(this.updateDate).asPromise()) || undefined;
	}

	/**
	 * Test if the item has a collection
	 * @returns {boolean} Boolean of whether the item has a collection.
	 * @memberof UmbDocumentItemDataResolver
	 */
	getHasCollection(): boolean {
		return this.getData()?.documentType.collection != undefined;
	}
}
