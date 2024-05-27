import { UmbBlockRteEntryElement } from './block-rte-entry.element.js';
import { css } from '@umbraco-cms/backoffice/external/lit';

/**
 * @element umb-rte-block
 * @element umb-rte-block-inline
 */
export class UmbBlockRteEntryInlineElement extends UmbBlockRteEntryElement {
	static styles = [
		...UmbBlockRteEntryElement.styles,
		css`
			:host {
				display: inline;
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
