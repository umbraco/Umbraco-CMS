import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-search-header-app')
export class UmbSearchHeaderAppElement extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: transparent;
			}
		`,
	];

	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (_instance) => {
			this._modalContext = _instance;
		});
	}

	#onSearchClick() {
		this._modalContext?.search();
	}

	render() {
		return html`
			<uui-button @click=${this.#onSearchClick} look="primary" label="search" compact>
				<uui-icon name="umb:search"></uui-icon>
			</uui-button>
		`;
	}
}

export default UmbSearchHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-header-app': UmbSearchHeaderAppElement;
	}
}
