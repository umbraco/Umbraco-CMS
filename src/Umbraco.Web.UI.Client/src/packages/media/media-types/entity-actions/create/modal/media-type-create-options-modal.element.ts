import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/index.js';
import type { UmbMediaTypeCreateOptionsModalData } from './index.js';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import {
	type UmbModalManagerContext,
	type UmbModalContext,
	UMB_MODAL_MANAGER_CONTEXT,
} from '@umbraco-cms/backoffice/modal';
import { UMB_FOLDER_CREATE_MODAL } from '@umbraco-cms/backoffice/tree';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-media-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbMediaTypeCreateOptionsModalData>;

	@property({ type: Object })
	data?: UmbMediaTypeCreateOptionsModalData;

	#modalContext?: UmbModalManagerContext;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	#onClick(event: PointerEvent) {
		event.stopPropagation();
		if (this.data?.parentKey === undefined) throw new Error('A parent unique is required to create a folder');

		const folderModalHandler = this.#modalContext?.open(UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
				parentUnique: this.data?.parentKey,
			},
		});
		folderModalHandler?.onSubmit().then(() => this.modalContext?.submit());
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this.modalContext?.submit();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Media Type">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${`section/settings/workspace/media-type/create/${this.data?.parentKey || 'null'}`}
						label="New Media Type..."
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-autofill"></uui-icon>
					</uui-menu-item>
					<uui-menu-item @click=${this.#onClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>
					</uui-menu-item>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this.#onCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbDataTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-create-options-modal': UmbDataTypeCreateOptionsModalElement;
	}
}
