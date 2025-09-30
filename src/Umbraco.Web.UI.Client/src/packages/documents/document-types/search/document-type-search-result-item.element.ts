import type { UmbDocumentTypeItemModel } from '../repository/types.js';
import { css, customElement, html, nothing, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

@customElement('umb-document-type-search-result-item')
export class UmbDocumentTypeSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbSearchResultItemModel & UmbDocumentTypeItemModel;

	override render() {
		if (!this.item) return nothing;

		return html`
			${when(
				this.item.icon,
				(icon) => html`<umb-icon name=${icon}></umb-icon>`,
				() => html`<uui-icon name="icon-document"></uui-icon>`,
			)}
			<span>${this.item.name}</span>
			<div class="extra">
				${when(
					this.item.isElement,
					() => html`
						<uui-tag look="secondary">
							<umb-localize key="contentTypeEditor_elementType">Element Type</umb-localize>
						</uui-tag>
					`,
				)}
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				border-radius: var(--uui-border-radius);
				outline-offset: -3px;
				padding: var(--uui-size-space-3) var(--uui-size-space-5);

				display: flex;
				gap: var(--uui-size-space-3);
				align-items: center;

				width: 100%;

				> span {
					flex: 1;
				}
			}
		`,
	];
}

export { UmbDocumentTypeSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-search-result-item': UmbDocumentTypeSearchResultItemElement;
	}
}
