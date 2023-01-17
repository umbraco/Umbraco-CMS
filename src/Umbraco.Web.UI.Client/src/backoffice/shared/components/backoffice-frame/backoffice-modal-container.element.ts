import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbModalHandler, UmbModalService, UMB_MODAL_SERVICE_CONTEXT_ALIAS } from '../../../../core/modal';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainer extends UmbLitElement {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				position: absolute;
			}
		`,
	];

	@state()
	private _modals?: UmbModalHandler[];

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_ALIAS, (modalService) => {
			this._modalService = modalService;
			this._observeModals();
		});
	}

	private _observeModals() {
		if (!this._modalService) return;

		this.observe(this._modalService.modals, (modals) => {
			this._modals = modals;
		});
	}

	render() {
		return html`
			<uui-modal-container>
				${this._modals ? repeat(this._modals, (modalHandler) => html`${modalHandler.element}`) : ''}
			</uui-modal-container>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-modal-container': UmbBackofficeModalContainer;
	}
}
