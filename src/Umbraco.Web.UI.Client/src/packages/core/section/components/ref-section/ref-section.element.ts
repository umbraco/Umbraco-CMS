import type { UmbSectionItemModel } from '../../repository/index.js';
import { UUIRefElement } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, css, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('umb-ref-section')
export class UmbRefSectionElement extends UmbElementMixin(UUIRefElement) {
	@property({ type: Object, attribute: false })
	item?: UmbSectionItemModel;

	public render() {
		return html`
			<div id="info">
				<div id="name">${this.item?.meta.label}</div>
			</div>
			<slot></slot>
			<slot name="actions" id="actions-container"></slot>
		`;
	}

	static styles = [
		...UUIRefElement.styles,
		css`
			#name {
				font-weight: 700;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-section': UmbRefSectionElement;
	}
}
