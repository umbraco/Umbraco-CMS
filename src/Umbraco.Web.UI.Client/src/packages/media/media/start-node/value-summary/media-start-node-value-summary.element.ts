import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-media-start-node-value-summary')
export class UmbMediaStartNodeValueSummaryElement extends UmbValueSummaryElementBase<UmbMediaItemModel | null> {
	override render() {
		return html`<span>${this._value?.name ?? this.localize.term('media_mediaRoot')}</span>`;
	}
}

export { UmbMediaStartNodeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-start-node-value-summary': UmbMediaStartNodeValueSummaryElement;
	}
}
