import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../core/context';
import type { ManifestEntityAction } from '../../../core/models';
import { UmbModalService } from '../../../core/services/modal';
import { UmbNodeStore } from '../../../core/stores/node.store';
import { UmbActionService } from '../actions.service';

@customElement('umb-tree-action-delete')
export default class UmbTreeActionDeleteElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _actionService?: UmbActionService;
	private _modalService?: UmbModalService;
	private _nodeStore?: UmbNodeStore;

	constructor() {
		super();

		this.consumeContext('umbActionService', (actionService: UmbActionService) => {
			this._actionService = actionService;
		});

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});

		this.consumeContext('umbNodeStore', (nodeStore: UmbNodeStore) => {
			this._nodeStore = nodeStore;
		});
	}

	private _handleLabelClick() {
		console.log(this.treeAction, 'label clicked');
		this._actionService?.openPage('umb-tree-action-delete-page');
		const modalHandler = this._modalService?.confirm({
			headline: 'Delete page 1',
			content: 'Are you sure you want to delete this page?',
			color: 'danger',
		});

		modalHandler?.onClose.then(({ confirmed }: any) => {
			if (confirmed && this._actionService) {
				this._nodeStore?.trash(this._actionService.key);
			}
		});
	}

	render() {
		return html` <uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-delete': UmbTreeActionDeleteElement;
	}
}
