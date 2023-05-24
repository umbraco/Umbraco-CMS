import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { DOCUMENT_TYPE_REPOSITORY_ALIAS } from '../../../repository/manifests.js';
import { UmbDocumentTypeCreateOptionsModalData } from '.';
import {
	UmbModalContext,
	UmbModalHandler,
	UMB_FOLDER_MODAL,
	UMB_MODAL_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-document-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbDocumentTypeCreateOptionsModalData>;

	@property({ type: Object })
	data?: UmbDocumentTypeCreateOptionsModalData;

	#modalContext?: UmbModalContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		const folderModalHandler = this.#modalContext?.open(UMB_FOLDER_MODAL, {
			repositoryAlias: DOCUMENT_TYPE_REPOSITORY_ALIAS,
		});
		folderModalHandler?.onSubmit().then(() => this.modalHandler?.submit());
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this.modalHandler?.submit();
	}

	#onCancel() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Document Type">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/document-type/create/${this.data?.parentKey || 'null'}`}
						label="New Document Type..."
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="umb:autofill"></uui-icon>}
					</uui-menu-item>
					<uui-menu-item @click=${this.#onClick} label="New Folder...">
						<uui-icon slot="icon" name="umb:folder"></uui-icon>}
					</uui-menu-item>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this.#onCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UUITextStyles];
}

export default UmbDataTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-create-options-modal': UmbDataTypeCreateOptionsModalElement;
	}
}
