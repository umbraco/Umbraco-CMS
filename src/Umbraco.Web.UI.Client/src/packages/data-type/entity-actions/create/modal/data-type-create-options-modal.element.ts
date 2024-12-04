import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/index.js';
import type { UmbDataTypeCreateOptionsModalData } from './constants.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-data-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbModalBaseElement<UmbDataTypeCreateOptionsModalData> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	override connectedCallback(): void {
		super.connectedCallback();
		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		// TODO: render the info from this instance in the list of actions
		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: {
				icon: 'icon-folder',
				label: 'New Folder...',
				folderRepositoryAlias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
			},
		});
	}

	#getCreateHref() {
		return `section/settings/workspace/data-type/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}`;
	}

	async #onCreateFolderClick(event: PointerEvent) {
		event.stopPropagation();

		try {
			await this.#createFolderAction?.execute();
			this._submitModal();
		} catch (error) {
			console.error(error);
		}
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Data Type">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item href=${this.#getCreateHref()} label="New Data Type..." @click=${this._submitModal}>
						<uui-icon slot="icon" name="icon-autofill"></uui-icon>
					</uui-menu-item>
					<uui-menu-item @click=${this.#onCreateFolderClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>}
					</uui-menu-item>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbDataTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-create-options-modal': UmbDataTypeCreateOptionsModalElement;
	}
}
