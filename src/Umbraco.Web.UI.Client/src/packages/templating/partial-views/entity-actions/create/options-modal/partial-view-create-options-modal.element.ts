import { UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL } from '../snippet-modal/index.js';
import { UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS } from '../../../constants.js';
import type { UmbPartialViewCreateOptionsModalData } from './index.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-partial-view-create-options-modal')
export class UmbPartialViewCreateOptionsModalElement extends UmbModalBaseElement<
	UmbPartialViewCreateOptionsModalData,
	string
> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	override connectedCallback(): void {
		super.connectedCallback();
		if (!this.data?.parent) throw new Error('A parent unique is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: {
				icon: 'icon-folder',
				label: this.localize.term('create_newFolder') + '...',
				folderRepositoryAlias: UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS,
			},
		});
	}

	async #onCreateFolderClick(event: PointerEvent) {
		event.stopPropagation();

		await this.#createFolderAction
			?.execute()
			.then(() => this._submitModal())
			.catch(() => undefined);
	}

	async #onCreateFromSnippetClick(event: PointerEvent) {
		event.stopPropagation();
		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		umbOpenModal(this, UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL, {
			data: {
				parent: this.data.parent,
			},
		})
			.then(() => this._submitModal())
			.catch(() => undefined);
	}

	// close the modal when navigating to data type
	#onNavigate() {
		this._submitModal();
	}

	#getCreateHref() {
		return `section/settings/workspace/partial-view/create/parent/${this.data?.parent.entityType}/${
			this.data?.parent.unique || 'null'
		}`;
	}

	override render() {
		return html`
			<umb-body-layout headline="Create Partial View">
				<uui-box>
					<!-- TODO: construct url -->
					<uui-menu-item
						href=${this.#getCreateHref()}
						label=${this.localize.term('create_newEmptyPartialView')}
						@click=${this.#onNavigate}>
						<uui-icon slot="icon" name="icon-document-html"></uui-icon>
					</uui-menu-item>

					<uui-menu-item
						@click=${this.#onCreateFromSnippetClick}
						label="${this.localize.term('create_newPartialViewFromSnippet')}...">
						<uui-icon slot="icon" name="icon-document-html"></uui-icon>
					</uui-menu-item>

					<uui-menu-item @click=${this.#onCreateFolderClick} label="${this.localize.term('create_newFolder')}...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>
					</uui-menu-item>
				</uui-box>

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbPartialViewCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-options-modal': UmbPartialViewCreateOptionsModalElement;
	}
}
