import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/index.js';
import type { UmbScriptCreateOptionsModalData } from './types.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-script-create-options-modal')
export class UmbScriptCreateOptionsModalElement extends UmbModalBaseElement<UmbScriptCreateOptionsModalData, string> {
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
				folderRepositoryAlias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
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
		return `section/settings/workspace/script/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}`;
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Script">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item href=${this.#getCreateHref()} label="New Javascript file" @click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-document-js"></uui-icon>}
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

export default UmbScriptCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-script-create-options-modal': UmbScriptCreateOptionsModalElement;
	}
}
