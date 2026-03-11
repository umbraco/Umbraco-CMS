import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { UmbCollectionActiveFilterModel } from '../filter/collection-filter.manager.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	#collectionContext?: typeof UMB_COLLECTION_CONTEXT.TYPE;

	@state()
	private _activeFilters?: Array<UmbCollectionActiveFilterModel> = [];

	#filterLabelsMap = new Map<string, string>();

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#collectionContext = context;

			this.observe(this.#collectionContext?.filtering.activeFilters, (activeFilters) => {
				this._activeFilters = activeFilters;
				if (!activeFilters || activeFilters.length === 0) return;

				this.observe(
					umbExtensionsRegistry.byTypeAndAliases(
						'collectionFilter',
						activeFilters.map((x) => x.alias),
					),
					(extensions) => {
						extensions.forEach((extension) => {
							const alias = extension.alias;
							const label = extension.meta?.label ?? alias;
							this.#filterLabelsMap.set(alias, label);
							this.requestUpdate();
						});
					},
				);
			});
		});
	}

	override render() {
		return html`
			<div id="top-row">
				<umb-collection-action-bundle></umb-collection-action-bundle>
				${this.#renderFilterExtensions()}
				<div id="slot"><slot></slot></div>
				<umb-collection-filter-bundle></umb-collection-filter-bundle>
				<umb-collection-view-bundle></umb-collection-view-bundle>
			</div>
			${this.#renderActiveFilters()}
		`;
	}

	#renderActiveFilters() {
		if (!this._activeFilters || this._activeFilters.length === 0) return;

		return html` <small>Showing result for:</small>

			${repeat(
				this._activeFilters,
				(activeFilter) => activeFilter.alias,
				(activeFilter) => this.#renderActiveFilterItem(activeFilter),
			)}`;
	}

	#renderActiveFilterItem(activeFilter: UmbCollectionActiveFilterModel) {
		const label = this.#filterLabelsMap.get(activeFilter.alias) ?? activeFilter.alias;
		return html`<div><span>${label}: ${activeFilter.value}</span></div>`;
	}

	#renderFilterExtensions() {
		return html`<umb-extension-with-api-slot single type="collectionTextFilter"></umb-extension-with-api-slot> `;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#top-row {
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				width: 100%;
			}
			#slot {
				flex: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
