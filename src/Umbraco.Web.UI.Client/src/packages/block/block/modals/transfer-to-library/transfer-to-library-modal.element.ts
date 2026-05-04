import type {
	UmbBlockTransferToLibraryModalData,
	UmbBlockTransferToLibraryModalValue,
} from './transfer-to-library-modal.token.js';
import { css, customElement, html, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-block-transfer-to-library-modal')
export class UmbBlockTransferToLibraryModalElement extends UmbModalBaseElement<
	UmbBlockTransferToLibraryModalData,
	UmbBlockTransferToLibraryModalValue
> {
	@state()
	private _name = '';

	@state()
	private _parentUnique: string | null = null;

	@state()
	private _hasSelectedLocation = false;

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
		return html`
			<umb-body-layout headline=${this.localize.term('blockEditor_transferToLibrary')}>
				<uui-box>
					<uui-form>
						<uui-form-layout-item>
							<uui-label slot="label" required>${this.localize.term('general_name')}</uui-label>
							<uui-input
								.value=${this._name}
								@input=${this.#onNameInput}
								label=${this.localize.term('general_name')}
								required></uui-input>
						</uui-form-layout-item>
						<uui-form-layout-item>
							<uui-label slot="label">${this.localize.term('general_location')}</uui-label>
							<umb-tree
								alias="Umb.Tree.Element"
								.props=${{
									hideTreeItemActions: true,
									foldersOnly: true,
								}}
								@selected=${this.#onFolderSelected}
								@deselected=${this.#onFolderDeselected}></umb-tree>
						</uui-form-layout-item>
					</uui-form>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_cancel')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('blockEditor_transferToLibrary')}
						look="primary"
						color="positive"
						?disabled=${!this._name.trim() || !this._hasSelectedLocation}
						@click=${this.#onTransfer}></uui-button>
				</div>
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

export default UmbBlockTransferToLibraryModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-transfer-to-library-modal': UmbBlockTransferToLibraryModalElement;
	}
}
