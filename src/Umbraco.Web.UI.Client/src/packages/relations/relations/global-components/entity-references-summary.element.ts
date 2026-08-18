import { UMB_ENTITY_REFERENCES_MODAL } from '../reference/modal/constants.js';
import type { UmbEntityReferenceRepository } from '../reference/types.js';
import type { UmbEntityReferenceListSource } from './entity-reference-list.element.js';
import type { UmbEntityReferencesConfig } from './types.js';
import { css, customElement, html, nothing, property, state, when } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';

/**
 * Publish/unpublish awareness: independent action buttons for the entities referencing the entity in `config`,
 * and for its descendants that are referenced elsewhere, each opening a paged overview of only that kind of
 * reference. Renders nothing when there are no references. Same `config` shape, getters, and `UmbChangeEvent`
 * contract as `umb-confirm-action-modal-entity-references`, so it can be used as a drop-in replacement wherever
 * that component's reference-aware gating (e.g. `disableUnpublishWhenReferenced`) is relied on.
 * @element umb-entity-references-summary
 */
@customElement('umb-entity-references-summary')
export class UmbEntityReferencesSummaryElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	config?: UmbEntityReferencesConfig;

	@state()
	private _totalReferencedByItems = 0;

	@state()
	private _totalDescendantsWithReferences = 0;

	#referenceRepository?: UmbEntityReferenceRepository;

	/**
	 * The number of items referencing the entity in `config`. `0` until the count has loaded.
	 * @returns {number} The referenced-by count.
	 */
	getTotalReferencedBy() {
		return this._totalReferencedByItems;
	}

	/**
	 * The number of descendants of the entity in `config` that are referenced elsewhere. `0` until the count
	 * has loaded, or if the reference repository does not support this lookup.
	 * @returns {number} The referenced-descendants count.
	 */
	getTotalDescendantsWithReferences() {
		return this._totalDescendantsWithReferences;
	}

	protected override firstUpdated(_changedProperties: PropertyValues): void {
		super.firstUpdated(_changedProperties);
		this.#initData();
	}

	async #initData() {
		if (!this.config) {
			this.#referenceRepository?.destroy();
			return;
		}

		if (!this.config.referenceRepositoryAlias) {
			throw new Error('Missing referenceRepositoryAlias in config.');
		}

		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.config.referenceRepositoryAlias,
		);

		await Promise.all([this.#loadReferencedByTotal(), this.#loadDescendantsWithReferencesTotal()]);
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #loadReferencedByTotal() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		if (!this.config?.unique) {
			throw new Error('Missing unique in config.');
		}

		// take: 1 — only the total is needed here, the overview modal fetches the actual items.
		const { data } = await this.#referenceRepository.requestReferencedBy(this.config.unique, 0, 1);
		this._totalReferencedByItems = data?.total ?? 0;
	}

	async #loadDescendantsWithReferencesTotal() {
		if (!this.#referenceRepository) {
			throw new Error('Failed to create reference repository.');
		}

		// If the repository does not have the method, we don't need to load the referenced descendants.
		if (!this.#referenceRepository.requestDescendantsWithReferences) return;

		if (!this.config?.unique) {
			throw new Error('Missing unique in config.');
		}

		const { data } = await this.#referenceRepository.requestDescendantsWithReferences(this.config.unique, 0, 1);
		this._totalDescendantsWithReferences = data?.total ?? 0;
	}

	#onClickView(source: UmbEntityReferenceListSource, event: Event) {
		event.preventDefault();
		if (!this.config) return;

		umbOpenModal(this, UMB_ENTITY_REFERENCES_MODAL, {
			data: {
				unique: this.config.unique,
				referenceRepositoryAlias: this.config.referenceRepositoryAlias,
				itemRepositoryAlias: this.config.itemRepositoryAlias,
				source,
			},
		}).catch(() => undefined);
	}

	override render() {
		const total = this._totalReferencedByItems + this._totalDescendantsWithReferences;
		if (total === 0) return nothing;

		return html`
			<p class="reference-summary">
				${when(
					this._totalReferencedByItems,
					() => html`
						<uui-button
							label=${this.localize.term('references_viewDependentItemsAction')}
							look="outline"
							@click=${(event: Event) => this.#onClickView('referencedBy', event)}>
							<umb-localize key="references_viewDependentItemsAction">View items that depend on this…</umb-localize>
						</uui-button>
					`,
				)}
				${when(
					this._totalDescendantsWithReferences,
					() => html`
						<uui-button
							label=${this.localize.term('references_viewDescendantsWithReferencesAction')}
							look="outline"
							@click=${(event: Event) => this.#onClickView('descendantsWithReferences', event)}>
							<umb-localize key="references_viewDescendantsWithReferencesAction"
								>View referenced descendants…</umb-localize
							>
						</uui-button>
					`,
				)}
			</p>
		`;
	}

	static override readonly styles = [
		css`
			.reference-summary {
				display: flex;
				align-items: center;
				flex-wrap: wrap;
				gap: var(--uui-size-space-2);
				color: var(--uui-color-text-alt);
			}
		`,
	];
}

export default UmbEntityReferencesSummaryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-references-summary': UmbEntityReferencesSummaryElement;
	}
}
