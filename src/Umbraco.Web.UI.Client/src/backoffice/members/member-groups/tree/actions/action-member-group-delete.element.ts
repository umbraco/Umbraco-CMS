import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import UmbTreeItemActionElement from '../../../../shared/components/tree/action/tree-item-action.element';
import {
	UmbMemberGroupDetailStore,
	UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN,
} from '../../member-group.detail.store';

@customElement('umb-tree-action-member-group-delete')
export default class UmbTreeActionMemberGroupDeleteElement extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	private _modalService?: UmbModalService;
	private _memberGroupDetailStore?: UmbMemberGroupDetailStore;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});

		this.consumeContext(UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN, (memberGroupDetailStore) => {
			this._memberGroupDetailStore = memberGroupDetailStore;
		});
	}

	private _handleLabelClick() {
		const modalHandler = this._modalService?.confirm({
			headline: `Delete ${this._activeTreeItem?.name ?? 'item'}`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});

		modalHandler?.onClose().then(({ confirmed }: any) => {
			if (confirmed && this._treeContextMenuService && this._memberGroupDetailStore && this._activeTreeItem) {
				this._memberGroupDetailStore?.trash([this._activeTreeItem.key]);
				this._treeContextMenuService.close();
			}
		});
	}

	render() {
		return html`<uui-menu-item label=${this.treeAction?.meta.label ?? ''} @click-label="${this._handleLabelClick}">
			<uui-icon slot="icon" name=${this.treeAction?.meta.icon ?? ''}></uui-icon>
		</uui-menu-item>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-member-group-delete': UmbTreeActionMemberGroupDeleteElement;
	}
}
