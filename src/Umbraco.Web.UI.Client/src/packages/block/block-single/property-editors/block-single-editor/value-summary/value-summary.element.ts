import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbBlockSingleValueModel } from '../../../types.js';

@customElement('umb-block-single-property-editor-value-summary')
export class UmbBlockSinglePropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<
	UmbBlockSingleValueModel | undefined
> {
	override render() {
		if (!this._value?.contentData?.length) return nothing;
		return html`<uui-icon name="icon-shape-square"></uui-icon>`;
	}
}

export { UmbBlockSinglePropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-single-property-editor-value-summary': UmbBlockSinglePropertyEditorValueSummaryElement;
	}
}
