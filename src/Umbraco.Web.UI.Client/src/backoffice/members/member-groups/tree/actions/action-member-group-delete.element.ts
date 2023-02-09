import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement } from 'lit/decorators.js';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '../../../../../core/modal';
import UmbTreeItemActionElement from '../../../../shared/components/tree/action/tree-item-action.element';
import { UmbMemberGroupTreeStore, UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN } from '../data/member-group.tree.store';

@customElement('umb-tree-action-member-group-delete')
export default class UmbTreeActionMemberGroupDeleteElement extends UmbTreeItemActionElement {
	static styles = [UUITextStyles, css``];

	private _modalService?: UmbModalService;
	private _memberGroupTreeStore?: UmbMemberGroupTreeStore;

	connectedCallback(): void {
		super.connectedCallback();

		this.consumeContext(UMB_MODAL_SERVICE_CONTEXT_TOKEN, (modalService) => {
			this._modalService = modalService;
		});

		this.consumeContext(UMB_MEMBER_GROUP_TREE_STORE_CONTEXT_TOKEN, (memberGroupTreeStore) => {
			this._memberGroupTreeStore = memberGroupTreeStore;
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
			if (confirmed && this._treeContextMenuService && this._memberGroupTreeStore && this._activeTreeItem) {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				/* @ts-ignore */
				// TODO: ignoring this error for now, because we will change this when entity actions are merged
				this._memberGroupTreeStore?.delete([this._activeTreeItem.key]);
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
