import type {
	UmbBlockTransferToElementLibraryModalData,
	UmbBlockTransferToElementLibraryModalValue,
} from './transfer-to-element-library-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-transfer-to-element-library-modal')
export class UmbBlockTransferToElementLibraryModalElement extends UmbModalBaseElement<
	UmbBlockTransferToElementLibraryModalData,
	UmbBlockTransferToElementLibraryModalValue
> {
	@state()
	private _name = '';

	@state()
	private _parentUnique: string | null = null;

	@state()
	private _hasSelectedLocation = false;

	override connectedCallback() {
		super.connectedCallback();
		if (this.data?.name) {
			this._name = this.data.name;
		}
	}

	#onNameInput(e: UUIInputEvent) {
		this._name = e.target.value as string;
	}

	#onFolderSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._parentUnique = event.unique ?? null;
		this._hasSelectedLocation = true;
	}

	#onFolderDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._parentUnique = null;
		this._hasSelectedLocation = false;
	}

	#onTransfer() {
		this.value = { name: this._name, parentUnique: this._parentUnique };
		this.modalContext?.submit();
	}

	override render() {
		const treeProps = { hideTreeItemActions: true, foldersOnly: true };
		return html`
			<umb-body-layout headline=${this.localize.term('blockEditor_transferToElementLibrary')}>
				<uui-box>
					<umb-property-layout label="#general_name" orientation="vertical" mandatory>
						<uui-input
							slot="editor"
							required
							label=${this.localize.term('general_name')}
							.value=${this._name}
							@input=${this.#onNameInput}
							${umbFocus()}>
						</uui-input>
					</umb-property-layout>
					<umb-property-layout label="#general_choose" orientation="vertical" mandatory>
						<umb-tree
							slot="editor"
							alias="Umb.Tree.Element"
							.props=${treeProps}
							@selected=${this.#onFolderSelected}
							@deselected=${this.#onFolderDeselected}>
						</umb-tree>
					</umb-property-layout>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
				<uui-button
					slot="actions"
					label=${this.localize.term('blockEditor_transferToElementLibrary')}
					look="primary"
					color="positive"
					?disabled=${!this._name.trim() || !this._hasSelectedLocation}
					@click=${this.#onTransfer}></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbBlockTransferToElementLibraryModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-transfer-to-element-library-modal': UmbBlockTransferToElementLibraryModalElement;
	}
}
