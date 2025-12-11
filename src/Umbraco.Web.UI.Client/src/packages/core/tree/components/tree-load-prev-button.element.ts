import { UmbTreeLoadMoreButtonElement } from './tree-load-more-button.element.js';
import { css, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-tree-load-prev-button')
export class UmbTreeLoadPrevButtonElement extends UmbTreeLoadMoreButtonElement {
	protected override _dataMark = 'tree:load-prev';

	static override readonly styles = [
		...UmbTreeLoadMoreButtonElement.styles,
		css`
			:host {
				margin-bottom: 0;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-load-prev-button': UmbTreeLoadPrevButtonElement;
	}
}
