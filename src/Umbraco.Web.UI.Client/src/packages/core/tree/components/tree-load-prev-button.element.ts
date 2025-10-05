import { CSSResult, css, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTreeLoadMoreButtonElement } from './tree-load-more-button.element';

@customElement('umb-tree-load-prev-button')
export class UmbTreeLoadPrevButtonElement extends UmbTreeLoadMoreButtonElement {
	protected override _dataMark = 'tree:load-prev';

	static override readonly styles: CSSResult[] = [
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
