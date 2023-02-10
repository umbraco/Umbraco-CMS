import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import UmbTreeItemActionElement from '../../../../shared/components/tree/action/tree-item-action.element';
import { UmbMemberTreeStore, UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN } from '../data/member.tree.store';

@customElement('umb-tree-action-member-delete')
export default class UmbTreeActionMemberDeleteElement extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	private _modalService?: UmbModalService;
	private _memberTreeStore?: UmbMemberTreeStore;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});

		this.consumeContext(UMB_MEMBER_TREE_STORE_CONTEXT_TOKEN, (memberTreeStore) => {
			this._memberTreeStore = memberTreeStore;
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
			if (confirmed && this._treeContextMenuService && this._memberTreeStore && this._activeTreeItem) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				/* @ts-ignore */
				// TODO: ignoring this error for now, because we will change this when entity actions are merged
				this._memberTreeStore?.delete([this._activeTreeItem.key]);
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
		'umb-tree-action-member-delete': UmbTreeActionMemberDeleteElement;
	}
}
