import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-media-start-node-value-summary')
export class UmbMediaStartNodeValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	value?: UmbMediaItemModel | null;

	override render() {
		return html`<span>${this.value?.name ?? this.localize.term('media_mediaRoot')}</span>`;
	}
}

export { UmbMediaStartNodeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-start-node-value-summary': UmbMediaStartNodeValueSummaryElement;
	}
}
