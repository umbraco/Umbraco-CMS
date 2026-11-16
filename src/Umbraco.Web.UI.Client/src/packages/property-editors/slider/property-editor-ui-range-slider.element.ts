import { UmbSliderPropertyEditorUIElementBase } from './property-editor-ui-slider-base.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-property-editor-ui-range-slider
 */
@customElement('umb-property-editor-ui-range-slider')
export class UmbRangeSliderPropertyEditorUIElement extends UmbSliderPropertyEditorUIElementBase {
	protected override readonly enableRange = true;
}

export { UmbRangeSliderPropertyEditorUIElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-range-slider': UmbRangeSliderPropertyEditorUIElement;
	}
}
