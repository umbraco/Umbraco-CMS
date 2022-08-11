import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, CSSResultGroup, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../core/context';
import { UmbModalHandler, UmbModalService } from '../../core/services/modal';

@customElement('umb-backoffice-modal-container')
export class UmbBackofficeModalContainer extends UmbContextConsumerMixin(LitElement) {
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
	private _modalSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
			this._modalSubscription?.unsubscribe();
			this._modalService?.modals.subscribe((modals: Array<UmbModalHandler>) => {
				this._modals = modals;
			});
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._modalSubscription?.unsubscribe();
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
