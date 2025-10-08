import { UmbDocumentVariantState } from '../types.js';
import type { UmbDocumentItemModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	type Observable,
	UmbArrayState,
	UmbBooleanState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { type UmbVariantContext, UMB_VARIANT_CONTEXT } from '@umbraco-cms/backoffice/variant';

type UmbDocumentItemDataResolverModel = Omit<UmbDocumentItemModel, 'parent' | 'hasChildren'>;

function isVariantsInvariant(variants: Array<{ culture: string | null }>): boolean {
	return variants?.[0]?.culture === null;
}
function findVariant<T extends { culture: string | null }>(variants: Array<T>, culture: string): T | undefined {
	return variants.find((x) => x.culture === culture);
}

/**
 * A controller for resolving data for a document item
 * @exports
 * @class UmbDocumentItemDataResolver
 * @augments {UmbControllerBase}
 */
export class UmbDocumentItemDataResolver<DataType extends UmbDocumentItemDataResolverModel> extends UmbControllerBase {
	#data = new UmbObjectState<DataType | undefined>(undefined);

	public readonly unique = this.#data.asObservablePart((x) => x?.unique);
	public readonly icon = this.#data.asObservablePart((x) => x?.documentType.icon);
	public readonly isTrashed = this.#data.asObservablePart((x) => x?.isTrashed);

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#state = new UmbStringState<DocumentVariantStateModel | null | undefined>(undefined);
	public readonly state = this.#state.asObservable() as Observable<DocumentVariantStateModel | null | undefined>;

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	#flags = new UmbArrayState<UmbEntityFlag>([], (data) => data.alias);
	public readonly flags = this.#flags.asObservable();

	#variantContext?: UmbVariantContext;
	#fallbackCulture?: string | null;
	#displayCulture?: string | null;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_VARIANT_CONTEXT, (context) => {
			this.#variantContext = context;
			this.#observeVariantContext();
		});
	}

	#observeVariantContext() {
		this.observe(
			this.#variantContext?.displayCulture,
			(displayCulture) => {
				if (displayCulture === undefined) return;
				this.#displayCulture = displayCulture;
				this.#setVariantAwareValues();
			},
			'umbObserveVariantId',
		);

		this.observe(
			this.#variantContext?.fallbackCulture,
			(fallbackCulture) => {
				if (fallbackCulture === undefined) return;
				this.#fallbackCulture = fallbackCulture;
				this.#setVariantAwareValues();
			},
			'umbObserveFallbackCulture',
		);
	}

	/**
	 * Get the current item
	 * @returns {DataType | undefined} The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	getData(): DataType | undefined {
		return this.#data.getValue();
	}

	/**
	 * Set the current item
	 * @param {DataType | undefined} data The current item
	 * @memberof UmbDocumentItemDataResolver
	 */
	setData(data: DataType | undefined) {
		this.#data.setValue(data);
		this.#setVariantAwareValues();
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
	async getState(): Promise<DocumentVariantStateModel | null | undefined> {
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

	#setVariantAwareValues() {
		if (!this.#variantContext) return;
		if (!this.#displayCulture) return;
		if (!this.#fallbackCulture) return;
		if (!this.#data) return;
		this.#setName();
		this.#setIsDraft();
		this.#setState();
		this.#setFlags();
	}

	#setName() {
		const variant = this.#getCurrentVariant();
		if (variant) {
			this.#name.setValue(variant.name);
			return;
		}

		const variants = this.getData()?.variants;
		if (variants) {
			const fallbackName = findVariant(variants, this.#fallbackCulture!)?.name;
			this.#name.setValue(`(${fallbackName})`);
		} else {
			this.#name.setValue(undefined);
		}
	}

	#setIsDraft() {
		const variant = this.#getCurrentVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this.#getCurrentVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	#setFlags() {
		const data = this.getData();
		if (!data) {
			this.#flags.setValue([]);
			return;
		}

		const flags = data.flags ?? [];
		const variantFlags = this.#getCurrentVariant()?.flags ?? [];
		this.#flags.setValue([...flags, ...variantFlags]);
	}

	#getCurrentVariant() {
		const variants = this.getData()?.variants;
		if (!variants) return undefined;

		if (isVariantsInvariant(variants)) {
			return variants[0];
		}

		return findVariant(variants, this.#displayCulture!);
	}
}
