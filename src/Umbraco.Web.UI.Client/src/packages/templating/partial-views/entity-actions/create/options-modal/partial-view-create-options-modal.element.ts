import { UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL } from '../snippet-modal/index.js';
import { UMB_PARTIAL_VIEW_FOLDER_REPOSITORY_ALIAS } from '../../../constants.js';
import type { UmbPartialViewCreateOptionsModalData } from './index.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/** @deprecated Use the `Umb.EntityAction.PartialView.Create` entity action with `entityCreateOptionAction` extensions instead. Scheduled for removal in Umbraco 19. */
@customElement('umb-partial-view-create-options-modal')
export class UmbPartialViewCreateOptionsModalElement extends UmbModalBaseElement<
	UmbPartialViewCreateOptionsModalData,
	string
> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	override connectedCallback(): void {
		super.connectedCallback();

		new UmbDeprecation({
			deprecated: 'UMB_PARTIAL_VIEW_CREATE_OPTIONS_MODAL and its associated modal element are deprecated.',
			removeInVersion: '19.0.0',
			solution:
				'Use the Umb.EntityAction.PartialView.Create entity action with entityCreateOptionAction extensions instead.',
		}).warn();

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
			<uui-dialog-layout headline=${this.localize.term('general_create')}>
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

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		css`
			uui-dialog-layout {
				--uui-menu-item-flat-structure: 1;
			}
		`,
	];
}

export default UmbPartialViewCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-partial-view-create-options-modal': UmbPartialViewCreateOptionsModalElement;
	}
}
