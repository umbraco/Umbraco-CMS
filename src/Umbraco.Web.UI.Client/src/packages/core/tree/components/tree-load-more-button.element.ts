import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-load-more-button')
export class UmbTreeLoadMoreButtonElement extends UmbLitElement {
	override render() {
		return html`<uui-button
			data-mark="tree:load-more"
			id="load-more"
			look="secondary"
			.label=${this.localize.term('actions_loadMore')}></uui-button>`;
	}

	static override readonly styles = css`
		:host {
			position: relative;
			display: block;
			padding-left: var(--uui-size-space-3);
			margin-right: var(--uui-size-space-2);
			margin-bottom: var(--uui-size-layout-2);
			margin-left: calc(var(--uui-menu-item-indent, 0) * var(--uui-size-4));
		}
		uui-button {
			width: 100%;
			height: var(--uui-size---uui-size-layout-3);
			--uui-box-border-radius: calc(var(--uui-border-radius) * 2);
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-load-more-button': UmbTreeLoadMoreButtonElement;
	}
}
