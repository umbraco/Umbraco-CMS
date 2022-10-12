import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbObserverMixin } from '../../core/observer';
import { UmbModalHandler, UmbModalService } from '../../core/services/modal';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainer extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles: CSSResultGroup = [
		UUITextStyles,
		css`
			:host {
				position: absolute;
			}
		`,
	];

	@state()
	private _modals: UmbModalHandler[] = [];

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
			this._observeModals();
		});
	}

	private _observeModals() {
		if (!this._modalService) return;

		this.observe<UmbModalHandler[]>(this._modalService.modals, (modals) => {
			this._modals = modals;
		});
	}

	render() {
		return html`
			<uui-modal-container>
				${repeat(this._modals, (modalHandler) => html`${modalHandler.element}`)}
			</uui-modal-container>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-modal-container': UmbBackofficeModalContainer;
	}
}
