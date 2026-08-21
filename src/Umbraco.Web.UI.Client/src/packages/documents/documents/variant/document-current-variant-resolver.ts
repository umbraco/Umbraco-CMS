import { UmbDocumentVariantState } from '../variant-state.js';
import { UmbArrayState, UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbVariantResolver } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityFlag } from '@umbraco-cms/backoffice/entity-flag';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * The variant fields {@link UmbDocumentCurrentVariantResolver} reads. Every document variant model
 * satisfies this, so a resolver can be built for item, tree item and detail models alike.
 * @interface UmbDocumentCurrentVariantResolverModel
 */
export interface UmbDocumentCurrentVariantResolverModel {
	name: string;
	culture: string | null;
	segment?: string | null;
	state: UmbDocumentVariantState | null;
	flags: Array<UmbEntityFlag>;
}

/**
 * Resolves which variant of a document applies to the ambient culture, and the name, state, draft status
 * and flags that follow from it. Composed by the document data resolvers so that every place a document
 * is displayed agrees on these rules. Anything a specific model carries beyond this is derived by the
 * composing resolver from {@link UmbDocumentCurrentVariantResolver.currentVariant}, which is typed to
 * that resolver's own variant model.
 * @exports
 * @class UmbDocumentCurrentVariantResolver
 * @augments {UmbControllerBase}
 * @template VariantModel
 */
export class UmbDocumentCurrentVariantResolver<
	VariantModel extends UmbDocumentCurrentVariantResolverModel = UmbDocumentCurrentVariantResolverModel,
> extends UmbControllerBase {
	#variantResolver: UmbVariantResolver<VariantModel>;
	#itemFlags: Array<UmbEntityFlag> = [];
	#hasVariants = false;

	#currentVariant = new UmbObjectState<VariantModel | undefined>(undefined);
	public readonly currentVariant = this.#currentVariant.asObservable();

	#name = new UmbStringState(undefined);
	public readonly name = this.#name.asObservable();

	#state = new UmbStringState<UmbDocumentVariantState | null | undefined>(undefined);
	public readonly state = this.#state.asObservable() as Observable<UmbDocumentVariantState | null | undefined>;

	#isDraft = new UmbBooleanState(undefined);
	public readonly isDraft = this.#isDraft.asObservable();

	#flags = new UmbArrayState<UmbEntityFlag>([], (data) => data.alias);
	public readonly flags = this.#flags.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.#variantResolver = new UmbVariantResolver<VariantModel>(this);

		// Recompute when either the ambient culture or the resolved variant changes. Observing the cultures
		// triggers a recompute when a culture arrives even if the matched variant is unchanged (clearing the
		// guard below); observing the variants ensures the recompute reads the freshly resolved variant.
		this.observe(this.#variantResolver.displayCulture, () => this.#resolve(), null);
		this.observe(this.#variantResolver.fallbackCulture, () => this.#resolve(), null);
		this.observe(this.#variantResolver.variant, () => this.#resolve(), null);
		this.observe(this.#variantResolver.fallbackVariant, () => this.#resolve(), null);
	}

	/**
	 * Set the variants to resolve from, together with the document level flags they are combined with.
	 * @param {Array<VariantModel> | undefined} variants The variants of the current document.
	 * @param {Array<UmbEntityFlag> | undefined} itemFlags The document level flags.
	 * @memberof UmbDocumentCurrentVariantResolver
	 */
	setVariants(variants: Array<VariantModel> | undefined, itemFlags: Array<UmbEntityFlag> | undefined) {
		this.#hasVariants = variants !== undefined;
		this.#itemFlags = itemFlags ?? [];
		this.#variantResolver.setVariants(variants);
		this.#resolve();
	}

	/**
	 * Get the display culture or fallback culture
	 * @returns {string | null | undefined} The display culture or fallback culture
	 * @memberof UmbDocumentCurrentVariantResolver
	 */
	getCulture(): string | null | undefined {
		return this.#variantResolver.getDisplayCulture() || this.#variantResolver.getFallbackCulture();
	}

	/**
	 * Get the variant matching the ambient culture, falling back to the fallback culture variant.
	 * @returns {VariantModel | undefined} The resolved variant.
	 * @memberof UmbDocumentCurrentVariantResolver
	 */
	getCurrentVariant(): VariantModel | undefined {
		return this.#variantResolver.getVariant() ?? this.#variantResolver.getFallbackVariant();
	}

	#resolve() {
		if (!this.#variantResolver.getDisplayCulture()) return;
		if (!this.#variantResolver.getFallbackCulture()) return;
		if (!this.#hasVariants) return;
		this.#setName();
		this.#setIsDraft();
		this.#setState();
		this.#setFlags();
		this.#currentVariant.setValue(this.getCurrentVariant());
	}

	#setName() {
		const variant = this.#variantResolver.getVariant();
		if (variant?.name) {
			this.#name.setValue(variant.name);
			return;
		}

		// Try fallback culture first, then first variant with any name
		const fallbackName =
			this.#variantResolver.getFallbackVariant()?.name ?? this.#variantResolver.getVariants().find((x) => x.name)?.name;

		if (fallbackName) {
			this.#name.setValue(`(${fallbackName})`);
			return;
		}

		this.#name.setValue('(Untitled)');
	}

	#setIsDraft() {
		const variant = this.#variantResolver.getVariant();
		const isDraft = variant?.state === UmbDocumentVariantState.DRAFT || false;
		this.#isDraft.setValue(isDraft);
	}

	#setState() {
		const variant = this.#variantResolver.getVariant();
		const state = variant?.state || UmbDocumentVariantState.NOT_CREATED;
		this.#state.setValue(state);
	}

	#setFlags() {
		const variantFlags = this.#variantResolver.getVariant()?.flags ?? [];
		this.#flags.setValue([...this.#itemFlags, ...variantFlags]);
	}
}
