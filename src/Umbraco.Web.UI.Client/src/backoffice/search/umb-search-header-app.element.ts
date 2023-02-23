import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-search-header-app')
export class UmbSearchHeaderApp extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			uui-button {
				font-size: 18px;
				--uui-button-background-color: transparent;
			}
		`,
	];

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (_instance) => {
			this._modalService = _instance;
		});
	}

	#onSearchClick() {
		this._modalService?.search();
	}

	render() {
		return html`
			<uui-button @click=${this.#onSearchClick} look="primary" label="search" compact>
				<uui-icon name="umb:search"></uui-icon>
			</uui-button>
		`;
	}
}

export default UmbSearchHeaderApp;

declare global {
	interface HTMLElementTagNameMap {
		'umb-search-header-app': UmbSearchHeaderApp;
	}
}
