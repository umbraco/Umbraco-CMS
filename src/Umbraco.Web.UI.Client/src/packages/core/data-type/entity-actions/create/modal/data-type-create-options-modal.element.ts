import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../repository/folder/manifests.js';
import { UmbDataTypeCreateOptionsModalData } from './index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	UmbModalManagerContext,
	UMB_FOLDER_CREATE_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-data-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbModalBaseElement<UmbDataTypeCreateOptionsModalData> {
	#modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		if (this.data?.parentUnique === undefined) throw new Error('A parent unique is required to create a folder');

		const folderModalHandler = this.#modalContext?.open(UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
				parentUnique: this.data.parentUnique,
			},
		});

		folderModalHandler?.onSubmit().then(() => this._submitModal());
	}

	render() {
		return html`
			<umb-body-layout headline="Create Data Type">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/data-type/create/${this.data?.parentUnique || null}`}
						label="New Data Type..."
						@click=${this._submitModal}>
						<uui-icon slot="icon" name="icon-autofill"></uui-icon>
					</uui-menu-item>
					<uui-menu-item @click=${this.#onClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>
					</uui-menu-item>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbDataTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-create-options-modal': UmbDataTypeCreateOptionsModalElement;
	}
}
