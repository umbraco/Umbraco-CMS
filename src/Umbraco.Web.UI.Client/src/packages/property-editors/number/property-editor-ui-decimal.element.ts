import { UmbPropertyEditorUINumberElement } from './property-editor-ui-number.element.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * Decimal variant of the number property editor UI. Overrides the default
 * step to 0.000001 — matching the six decimal places supported by the
 * underlying `DECIMAL(38,6)` database column — so values with fractional
 * parts are accepted by the input when no step is configured.
 */
@customElement('umb-property-editor-ui-decimal')
export class UmbPropertyEditorUIDecimalElement extends UmbPropertyEditorUINumberElement {
	protected override defaultStep = 0.000001;
}

export default UmbPropertyEditorUIDecimalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-decimal': UmbPropertyEditorUIDecimalElement;
	}
}
