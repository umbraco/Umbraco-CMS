import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import { UmbContextProviderMixin } from '../../../core/context';

import { ActionPageEntity } from './action.element';
import { UmbActionPageService } from '.';
import '.';

// import './actions-modal.element';
// import './tree-action-create-page.element';
// import './tree-action-create-page-2.element';
// import './action-page.service';
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

	@query('umb-action-page-service')
	private _actionPageService!: UmbActionPageService;

	@state()
	private _modalOpen = false;

	@state()
	private entity: { name: string; key: string } = { name: '', key: '' };

	connectedCallback() {
		super.connectedCallback();
		this.provideContext('umbActionService', this);
	}

	public open(entity: ActionPageEntity) {
		this.entity = entity;
		this._modalOpen = true;
	}

	public close() {
		this._modalOpen = false;
	}

	private _renderBackdrop() {
		// eslint-disable-next-line lit-a11y/click-events-have-key-events
		return this._modalOpen ? html`<div id="backdrop" @click=${this.close}></div>` : nothing;
	}

	private _renderModal() {
		return this._modalOpen
			? html`<umb-action-page-service id="action-modal" .actionEntity=${this.entity}></umb-action-page-service>`
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
