import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbBlockGridValueModel } from '../../../types.js';

@customElement('umb-block-grid-property-editor-value-summary')
export class UmbBlockGridPropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	UmbBlockGridValueModel | undefined
> {
	override render() {
		const count = this._value?.contentData?.length ?? 0;
		if (!count) return nothing;
		return html`<span>${count}</span> <uui-icon name="icon-layout-masonry"></uui-icon>`;
	}
}

export { UmbBlockGridPropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-property-editor-value-summary': UmbBlockGridPropertyEditorValueSummaryElement;
	}
}
