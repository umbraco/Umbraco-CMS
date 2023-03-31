import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { DATA_TYPE_REPOSITORY_ALIAS } from '../../../repository/manifests';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UmbModalContext, UMB_FOLDER_MODAL, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

@customElement('umb-create-data-type-modal')
export class UmbCreateDataTypeModalElement extends UmbModalBaseElement {
	static styles = [UUITextStyles];

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
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
		});
		folderModalHandler?.onSubmit().then(() => this.modalHandler?.submit());
	}

	#onCancel() {
		this.modalHandler?.reject();
	}

	render() {
		return html`
			<umb-body-layout headline="Create Data Type">
				<uui-box>
					<uui-menu-item href="" label="New Data Type...">
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
}

export default UmbCreateDataTypeModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-data-type-modal': UmbCreateDataTypeModalElement;
	}
}
