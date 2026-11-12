import { UMB_FILTER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/filter';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, nothing, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import '../../filter/filter.element.js';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	@state()
	private _filters: Array<any> = [];

	@state()
	private _totalActiveFilters = 0;

	constructor() {
		super();

		new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'filter',
			undefined,
			undefined,
			(filters) => {
				this._filters = filters;
			},
		);

		this.consumeContext(UMB_FILTER_MANAGER_CONTEXT, (manager) => {
			if (!manager) return;

			this.observe(manager.totalActiveFilters, (total) => {
				this._totalActiveFilters = total ?? 0;
			});
		});
	}

	override render() {
		if (!this._filters.length) return nothing;

		return html`
			<uui-button compact popovertarget="collection-filter-bundle-popover" label="Filters">
				<umb-icon name="icon-equalizer"></umb-icon>
				Filters ${this._totalActiveFilters > 0 ? html`<uui-badge>${this._totalActiveFilters}</uui-badge>` : nothing}
			</uui-button>
			<uui-popover-container id="collection-filter-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${repeat(
							this._filters,
							(filter) => filter.alias,
							(filter) => html`
								<umb-filter .alias=${filter.alias} .label=${filter.manifest?.meta?.label}
									>${filter.component}</umb-filter
								>
							`,
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
