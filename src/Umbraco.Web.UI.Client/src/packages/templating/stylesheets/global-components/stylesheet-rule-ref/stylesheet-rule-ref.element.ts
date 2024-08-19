import { UUIRefNodeElement } from '@umbraco-cms/backoffice/external/uui';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 *  @element umb-stylesheet-rule-ref
 *  @description - Component for displaying a reference to a stylesheet rule
 *  @augments UUIRefNodeElement
 */
@customElement('umb-stylesheet-rule-ref')
export class UmbStylesheetRuleRefElement extends UUIRefNodeElement {
	protected override fallbackIcon =
		'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3H7a2 2 0 0 0-2 2v5a2 2 0 0 1-2 2 2 2 0 0 1 2 2v5c0 1.1.9 2 2 2h1" /><path d="M16 21h1a2 2 0 0 0 2-2v-5c0-1.1.9-2 2-2a2 2 0 0 1-2-2V5a2 2 0 0 0-2-2h-1" /></svg>';

	static override styles = [...UUIRefNodeElement.styles];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-rule-ref': UmbStylesheetRuleRefElement;
	}
}
