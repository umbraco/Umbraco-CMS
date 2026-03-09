import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-filter-bundle')
export class UmbCollectionFilterBundleElement extends UmbLitElement {
	override render() {
		return html`
			<uui-button compact popovertarget="collection-filter-bundle-popover" label="Filters">
				<umb-icon name="icon-filter"></umb-icon>
			</uui-button>
			<uui-popover-container id="collection-filter-bundle-popover" placement="bottom-end">
				<umb-popover-layout>
					<div class="filter-dropdown">
						<umb-extension-with-api-slot type="collectionFilter"></umb-extension-with-api-slot>
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
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-filter-bundle': UmbCollectionFilterBundleElement;
	}
}
