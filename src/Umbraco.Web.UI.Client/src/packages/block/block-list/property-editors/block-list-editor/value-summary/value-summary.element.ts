import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbBlockListValueModel } from '../../../types.js';

@customElement('umb-block-list-value-summary')
export class UmbBlockListValueSummaryElement extends UmbValueSummaryElementBase<UmbBlockListValueModel | undefined> {
	override render() {
		const count = this._value?.contentData?.length ?? 0;
		if (!count) return nothing;
		return html`<span>${count}</span> <uui-icon name="icon-layout-list"></uui-icon>`;
	}
}

export { UmbBlockListValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-list-value-summary': UmbBlockListValueSummaryElement;
	}
}
