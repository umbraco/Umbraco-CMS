import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-media-user-start-node-value-summary')
export class UmbMediaUserStartNodeValueSummaryElement extends UmbValueSummaryElementBase<UmbMediaItemModel | null> {
	override render() {
		if (this._value === undefined) return nothing;
		return html`<span>${this._value?.name ?? this.localize.term('media_mediaRoot')}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-user-start-node-value-summary': UmbMediaUserStartNodeValueSummaryElement;
	}
}
