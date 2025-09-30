import { UmbBlockRteEntryElement } from './block-rte-entry.element.js';
import { css, customElement } from '@umbraco-cms/backoffice/external/lit';

/**
 * Implementation for the inline component, inherited code from the umb-rte-block element.
 * @element umb-rte-block-inline
 */
@customElement('umb-rte-block-inline')
export class UmbBlockRteEntryInlineElement extends UmbBlockRteEntryElement {
	static override styles = [
		...UmbBlockRteEntryElement.styles,
		css`
			:host {
				display: inline-block;
			}
		`,
	];
}

export default UmbBlockRteEntryInlineElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rte-block-inline': UmbBlockRteEntryInlineElement;
	}
}
