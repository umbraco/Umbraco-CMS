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
 * Renders one action button per kind of reference to the entity in `config`: one for the items referencing it,
 * another for its descendants that are referenced elsewhere. Each button opens a paged overview of only that
 * kind of reference. Renders nothing when there are no references. Dispatches a `UmbChangeEvent` once both
 * totals have loaded (reporting no references if the lookup fails), reloads when `config` changes, and exposes `getTotalReferencedBy()` / `getTotalDescendantsWithReferences()`, so a host
 * can gate an action on the result — for example, a publish or unpublish confirmation dialog.
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
	#referenceRepositoryAlias?: string;
	#loadToken = 0;

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

	protected override updated(changedProperties: PropertyValues): void {
		super.updated(changedProperties);
		if (changedProperties.has('config')) {
			this.#initData();
		}
	}

	async #initData() {
		const token = ++this.#loadToken;
		const config = this.config;
		let totals = { referencedBy: 0, descendants: 0 };

		if (config) {
			try {
				totals = await this.#loadTotals(config);
			} catch (error) {
				// Fail open: a host gating an action on these totals should proceed, not wait forever for a change event.
				console.error('Failed to load entity references:', error);
			}
		}

		// A newer config has since been set — its result should win, not ours.
		if (token !== this.#loadToken) return;

		this._totalReferencedByItems = totals.referencedBy;
		this._totalDescendantsWithReferences = totals.descendants;

		if (config) {
			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	async #loadTotals(config: UmbEntityReferencesConfig) {
		const repository = await this.#getReferenceRepository(config.referenceRepositoryAlias);

		// take: 1 — only the totals are needed here, the overview modal fetches the actual items.
		const [referencedBy, descendants] = await Promise.all([
			repository.requestReferencedBy(config.unique, 0, 1),
			// If the repository does not have the method, there are no referenced descendants to load.
			repository.requestDescendantsWithReferences?.(config.unique, 0, 1),
		]);

		return { referencedBy: referencedBy.data?.total ?? 0, descendants: descendants?.data?.total ?? 0 };
	}

	async #getReferenceRepository(alias: string): Promise<UmbEntityReferenceRepository> {
		if (this.#referenceRepository && this.#referenceRepositoryAlias === alias) return this.#referenceRepository;

		this.#referenceRepository?.destroy();
		this.#referenceRepository = await createExtensionApiByAlias<UmbEntityReferenceRepository>(this, alias);
		this.#referenceRepositoryAlias = alias;
		return this.#referenceRepository;
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
