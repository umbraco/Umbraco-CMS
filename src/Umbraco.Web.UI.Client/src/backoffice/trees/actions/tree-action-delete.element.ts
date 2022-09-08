import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import type { ManifestEntityAction } from '../../../core/models';
import { UmbModalService } from '../../../core/services/modal';
import { UmbNodeStore } from '../../../core/stores/node.store';
import UmbActionElement from './action.element';

@customElement('umb-tree-action-delete')
export class UmbTreeActionDeleteElement extends UmbActionElement {
	static styles = [UUITextStyles, css``];

	@property({ attribute: false })
	public treeAction?: ManifestEntityAction;

	private _modalService?: UmbModalService;
	private _nodeStore?: UmbNodeStore;

	constructor() {
		super();

		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});

		this.consumeContext('umbNodeStore', (nodeStore: UmbNodeStore) => {
			this._nodeStore = nodeStore;
		});
	}

	private _handleLabelClick() {
		const modalHandler = this._modalService?.confirm({
			headline: 'Delete page 1',
			content: 'Are you sure you want to delete this page?',
			color: 'danger',
		});

		modalHandler?.onClose.then(({ confirmed }: any) => {
			if (confirmed && this._actionService) {
				this._nodeStore?.trash(this._entity.key);
				this._actionService.close();
			}
		});
	}

	render() {
		return html` <uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

export default UmbTreeActionDeleteElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-delete': UmbTreeActionDeleteElement;
	}
}
