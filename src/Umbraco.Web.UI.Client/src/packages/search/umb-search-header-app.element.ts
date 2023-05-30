import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, CSSResultGroup, html , customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-search-header-app')
export class UmbSearchHeaderAppElement extends UmbLitElement {
	private _modalContext?: UmbModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (_instance) => {
			this._modalContext = _instance;
		});
	}

	#onSearchClick() {
		alert('implement search search modal');
	}

	render() {
		return html`
			<uui-button @click=${this.#onSearchClick} look="primary" label="search" compact>
				<uui-icon name="umb:search"></uui-icon>
			</uui-button>
		`;
	}

	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: transparent;
			}
		`,
	];
}

export default UmbSearchHeaderAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-header-app': UmbSearchHeaderAppElement;
	}
}
