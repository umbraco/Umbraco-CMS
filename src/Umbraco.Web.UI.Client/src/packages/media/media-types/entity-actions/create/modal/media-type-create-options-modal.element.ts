import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../constants.js';
import type { UmbMediaTypeCreateOptionsModalData } from './constants.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * @deprecated This element is deprecated and will be removed in v.17.0.0.
 * Please use the UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL instead.
 * @exports
 * @class UmbMediaTypeCreateOptionsModalElement
 * @augments {UmbModalBaseElement<UmbDataTypeCreateOptionsModalData>}
 */
@customElement('umb-media-type-create-options-modal')
export class UmbMediaTypeCreateOptionsModalElement extends UmbModalBaseElement<UmbMediaTypeCreateOptionsModalData> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	override connectedCallback(): void {
		super.connectedCallback();
		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		new UmbDeprecation({
			deprecated: 'umb-media-type-create-options-modal',
			removeInVersion: '17.0.0',
			solution: 'Use UMB_ENTITY_CREATE_OPTION_ACTION_LIST_MODAL instead',
		}).warn();

		// TODO: render the info from this instance in the list of actions
		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: {
				icon: 'icon-folder',
				label: 'New Folder...',
				folderRepositoryAlias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
			},
		});
	}

	async #onCreateFolderClick(event: PointerEvent) {
		event.stopPropagation();

		try {
			await this.#createFolderAction?.execute();
			this._submitModal();
		} catch (error) {
			return;
		}
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this.modalContext?.submit();
	}

	#onCancel() {
		this.modalContext?.reject();
	}

	#getCreateHref() {
		return `section/settings/workspace/media-type/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}`;
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Media Type">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item href=${this.#getCreateHref()} label="New Media Type..." @click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-autofill"></uui-icon>
					</uui-menu-item>
					<uui-menu-item @click=${this.#onCreateFolderClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>}
					</uui-menu-item>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this.#onCancel}">Cancel</uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbMediaTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-create-options-modal': UmbMediaTypeCreateOptionsModalElement;
	}
}
