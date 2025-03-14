import { UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL } from './property-type-modal/property-type-modal.token.js';
import type {
	UmbDocumentValueGranularUserPermissionFlowModalData,
	UmbDocumentValueGranularUserPermissionFlowModalValue,
} from './document-value-granular-permission-flow-modal.token.js';
import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_DOCUMENT_TYPE_TREE_ALIAS } from '@umbraco-cms/backoffice/document-type';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-value-granular-user-permission-flow-modal')
export class UmbDocumentValueGranularUserPermissionFlowModalElement extends UmbModalBaseElement<
	UmbDocumentValueGranularUserPermissionFlowModalData,
	UmbDocumentValueGranularUserPermissionFlowModalValue
> {
	@state()
	_selection: Array<string> = [];

	async #next() {
		if (this._selection.length === 0) {
			throw new Error('No document type selected');
		}

		const documentType = { unique: this._selection[0] };

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) {
			throw new Error('Could not get modal manager context');
		}

		const modal = modalManager.open(this, UMB_DOCUMENT_VALUE_GRANULAR_USER_PERMISSION_FLOW_PROPERTY_TYPE_MODAL, {
			data: {
				documentType,
			},
		});

		try {
			const value = await modal.onSubmit();

			this.updateValue({
				documentType,
				propertyType: value.propertyType,
				verbs: value.verbs,
			});

			this._submitModal();
		} catch {}
	}

	#onTreeSelectionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const selection = target.getSelection();
		this._selection = [...selection];
	}

	override render() {
		return html`
			<umb-body-layout headline="Choose Document Type">
				<uui-box>
					<umb-tree
						@selection-change=${this.#onTreeSelectionChange}
						.props=${{
							hideTreeRoot: true,
							/*
						pickableFilter: (treeItem: UmbDocumentTypeTreeItemModel) =>
							!this._items?.map((i) => i.unique).includes(treeItem.unique),
						*/
						}}
						alias=${UMB_DOCUMENT_TYPE_TREE_ALIAS}></umb-tree>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label="${this.localize.term('general_next')}"
						look="primary"
						color="positive"
						@click=${this.#next}
						?disabled=${this._selection.length === 0}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}
}

export { UmbDocumentValueGranularUserPermissionFlowModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-value-granular-user-permission-flow-modal': UmbDocumentValueGranularUserPermissionFlowModalElement;
	}
}
