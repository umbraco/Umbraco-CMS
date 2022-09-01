import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../core/context';
import type { ManifestEntityAction } from '../../core/models';

import './actions-modal.element';
import './actions/tree-action-create-page.element';
import './actions/tree-action-create-page-2.element';
// TODO how do we dynamically import this so we don't have to import every page that could potentially be used?

@customElement('umb-action-service')
export class UmbActionService extends UmbContextProviderMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				z-index: 1;
			}
			#backdrop {
				content: '';
				position: absolute;
				inset: 0px;
				background-color: black;
				opacity: 0.5;
				width: 100vw;
				height: 100vh;
				z-index: -1;
			}
			#relative-wrapper {
				background-color: var(--uui-color-surface);
				position: relative;
				display: flex;
				flex-direction: column;
				width: 100%;
				height: 100%;
			}
			#action-modal {
				position: absolute;
				left: 300px;
				height: 100%;
				z-index: 1;
				top: 0;
				width: 300px;
				border: none;
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
			}
		`,
	];

	@state()
	private _modalOpen = false;

	@state()
	private _name = '';

	public key = '';

	@state()
	private _pages: Array<HTMLElement> = [];

	connectedCallback() {
		super.connectedCallback();
		this.provideContext('umbActionService', this);
	}

	public open(name: string, key: string) {
		this._name = name;
		this.key = key;
		this._modalOpen = true;
	}

	public close() {
		this._modalOpen = false;
		this._pages = [];
	}

	public openPage(elementName: string) {
		const element = document.createElement(elementName);
		this._pages.push(element);
		this.requestUpdate('_pages');
	}

	public closeTopPage() {
		this._pages.pop();
		this.requestUpdate('_pages');
	}

	private _renderTopPage() {
		if (this._pages.length === 0) {
			return nothing;
		}

		return this._pages[this._pages.length - 1];
	}

	private _renderBackdrop() {
		// eslint-disable-next-line lit-a11y/click-events-have-key-events
		return this._modalOpen ? html`<div id="backdrop" @click=${this.close}></div>` : nothing;
	}

	private _renderModal() {
		return this._modalOpen
			? html` <div id="action-modal">
					${this._pages.length === 0
						? html`<umb-actions-modal .name=${this._name}></umb-actions-modal>`
						: this._renderTopPage()}
			  </div>`
			: nothing;
	}

	render() {
		return html`
			${this._renderBackdrop()}
			<div id="relative-wrapper">
				<slot></slot>
				${this._renderModal()}
			</div>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-action-service': UmbActionService;
	}
}
