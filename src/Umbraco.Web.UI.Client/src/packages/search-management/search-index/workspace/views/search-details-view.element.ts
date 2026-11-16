import type { ManifestSearchIndexDetailBox } from '../../index-detail-box/types.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-search-details-view')
export class UmbSearchDetailsViewElement extends UmbLitElement {
	override render() {
		return html`
			<div class="container">
				<div class="column">
					<umb-extension-slot
						type="searchIndexDetailBox"
						.filter=${(ext: ManifestSearchIndexDetailBox) => ext.meta?.column === 'left'}></umb-extension-slot>
				</div>
				<div class="column">
					<umb-extension-slot
						type="searchIndexDetailBox"
						.filter=${(ext: ManifestSearchIndexDetailBox) => ext.meta?.column !== 'left'}></umb-extension-slot>
				</div>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.container {
				display: grid;
				grid-template-columns: 1fr 350px;
				gap: var(--uui-size-layout-1);
			}

			.column {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbSearchDetailsViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-details-view': UmbSearchDetailsViewElement;
	}
}
