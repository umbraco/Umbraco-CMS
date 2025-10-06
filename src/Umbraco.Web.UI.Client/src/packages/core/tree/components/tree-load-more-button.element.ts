import { CSSResult, css, customElement, html, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-tree-load-more-button')
export class UmbTreeLoadMoreButtonElement extends UmbLitElement {
	@property({ type: Boolean })
	loading: boolean = false;

	protected _dataMark: string = 'tree:load-more';

	override render() {
		const state: UUIButtonState = this.loading ? 'waiting' : undefined;
		const label = this.localize.term('actions_showMore');

		return html`<uui-button
			state=${ifDefined(state)}
			data-mark=${this._dataMark}
			id="load-more"
			.label=${label}
			.title=${label}
			>…</uui-button
		>`;
	}

	static override readonly styles: CSSResult[] = [
		css`
			:host {
				position: relative;
				display: block;
				padding-left: var(--uui-size-space-3);
				margin-right: var(--uui-size-space-2);
				margin-left: calc(var(--uui-menu-item-indent, 0) * var(--uui-size-4));
				margin-bottom: var(--uui-size-layout-2);
			}

			uui-button {
				width: 100%;
				height: var(--uui-size---uui-size-layout-3);
				--uui-box-border-radius: calc(var(--uui-border-radius) * 2);
				--uui-button-content-align: start;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-load-more-button': UmbTreeLoadMoreButtonElement;
	}
}
