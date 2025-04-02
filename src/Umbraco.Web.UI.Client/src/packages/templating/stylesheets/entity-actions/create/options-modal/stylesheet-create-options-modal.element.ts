import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/index.js';
import type { UmbStylesheetCreateOptionsModalData } from './stylesheet-create-options.modal-token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

/** @deprecated No longer used internally. This will be removed in Umbraco 18. [LK] */
@customElement('umb-stylesheet-create-options-modal')
export class UmbStylesheetCreateOptionsModalElement extends UmbModalBaseElement<
	UmbStylesheetCreateOptionsModalData,
	string
> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	override connectedCallback(): void {
		super.connectedCallback();
		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: {
				icon: 'icon-folder',
				label: 'New folder...',
				folderRepositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
			},
		});
	}

	async #onCreateFolderClick(event: PointerEvent) {
		event.stopPropagation();

		this.#createFolderAction
			?.execute()
			.then(() => {
				this._submitModal();
			})
			.catch(() => {});
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	#getCreateHref() {
		return `section/settings/workspace/stylesheet/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}`;
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Stylesheet">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item href=${`${this.#getCreateHref()}/view/code`} label="New Stylesheet" @click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-palette"></uui-icon>}
					</uui-menu-item>

					<uui-menu-item
						href=${`${this.#getCreateHref()}/view/rich-text-editor`}
						label="New Rich Text Editor Stylesheet"
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-palette"></uui-icon>}
					</uui-menu-item>

					<uui-menu-item @click=${this.#onCreateFolderClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>}
					</uui-menu-item>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbStylesheetCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-stylesheet-create-options-modal': UmbStylesheetCreateOptionsModalElement;
	}
}
