import { UMB_FILTER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/filter';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_COLLECTION_CONTEXT } from '../default/index.js';
import type { ManifestCollectionFilter } from '../filter/collection-filter.extension.js';

const FILTERS_CONTROLLER_ALIAS = 'umbCollectionFilterExtensions';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	@state()
	private _filters: Array<ManifestCollectionFilter> = [];

	@state()
	private _totalActiveValues = 0;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (context) => {
			this.#observeFilters(context?.manifest?.alias);
		});

		this.consumeContext(UMB_FILTER_MANAGER_CONTEXT, (manager) => {
			if (!manager) return;

			this.observe(manager.totalActiveValues, (total) => {
				this._totalActiveValues = total ?? 0;
			});
		});
	}

	#observeFilters(collectionAlias: string | undefined) {
		this.removeUmbControllerByAlias(FILTERS_CONTROLLER_ALIAS);
		this._filters = [];
		if (!collectionAlias) return;

		new UmbExtensionsManifestInitializer<ManifestCollectionFilter, 'collectionFilter'>(
			this,
			umbExtensionsRegistry,
			'collectionFilter',
			(manifest) => manifest.forCollections.includes(collectionAlias),
			(filters) => {
				this._filters = filters.map((filter) => filter.manifest);
			},
			FILTERS_CONTROLLER_ALIAS,
		);
	}

	override render() {
		if (!this._filters.length) return nothing;

		return html`
			<uui-button
				compact
				popovertarget="collection-filter-bundle-popover"
				label="Filters"
				data-mark="collection:filters">
				<umb-icon name="icon-equalizer"></umb-icon>
				Filters ${this._totalActiveValues > 0 ? html`<uui-badge>${this._totalActiveValues}</uui-badge>` : nothing}
			</uui-button>
			<uui-popover-container id="collection-filter-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._filters,
							(filter) => filter.alias,
							(filter) => html`<umb-filter .manifest=${filter}></umb-filter>`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}

			.filter-dropdown {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-5);
				padding: var(--uui-size-space-5);
				min-width: 250px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-bundle': UmbCollectionFilterBundleElement;
	}
}
