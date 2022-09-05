import { LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../core/context';
import { UmbActionPageService } from './action-page.service';
import { UmbActionService } from './actions.service';

export type ActionPageEntity = {
	key: string;
	name: string;
};

@customElement('umb-action')
export default class UmbActionElement extends UmbContextConsumerMixin(LitElement) {
	@state()
	protected _entity: ActionPageEntity = { name: '', key: '' };

	protected _actionService?: UmbActionService;
	protected _actionPageService?: UmbActionPageService;
	private _actionPageSubscription?: Subscription;

	connectedCallback() {
		super.connectedCallback();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});

		this.consumeContext('umbActionPageService', (actionPageService: UmbActionPageService) => {
			this._actionPageService = actionPageService;

			this._actionPageSubscription?.unsubscribe();
			this._actionPageService?.entity.subscribe((entity: ActionPageEntity) => {
				this._entity = entity;
				console.log('entity changed', this._entity);
			});
		});
	}

	disconnectCallback() {
		super.disconnectedCallback();
		this._actionPageSubscription?.unsubscribe();
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-action': UmbActionElement;
	}
}
