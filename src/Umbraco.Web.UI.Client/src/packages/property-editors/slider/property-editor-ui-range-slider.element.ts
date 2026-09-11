import { UmbPropertyEditorUISliderElementBase } from './property-editor-ui-slider-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-range-slider
 */
@customElement('umb-property-editor-ui-range-slider')
export class UmbPropertyEditorUIRangeSliderElement extends UmbPropertyEditorUISliderElementBase {
	protected override readonly enableRange = true;
}

export default UmbPropertyEditorUIRangeSliderElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-range-slider': UmbPropertyEditorUIRangeSliderElement;
	}
}
