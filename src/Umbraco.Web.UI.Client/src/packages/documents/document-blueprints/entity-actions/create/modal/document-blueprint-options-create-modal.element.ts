import {
	UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
	UmbDocumentBlueprintFolderRepository,
} from '../../../tree/index.js';
import type {
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue,
} from './constants.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import {
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	type UmbDocumentTypeTreeItemModel,
} from '@umbraco-cms/backoffice/document-type';

@customElement('umb-document-blueprint-options-create-modal')
export class UmbDocumentBlueprintOptionsCreateModalElement extends UmbModalBaseElement<
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue
> {
	@state()
	private _parentName?: string;

	#createFolderAction?: UmbCreateFolderEntityAction;

	#folderRepository = new UmbDocumentBlueprintFolderRepository(this);

	override async connectedCallback(): Promise<void> {
		super.connectedCallback();
		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		if (this.data.parent.unique) {
			const { data: parent } = await this.#folderRepository.requestByUnique(this.data.parent.unique.toString());
			this._parentName = parent?.name ?? this.localize.term('general_unknown');
		} else {
			this._parentName = this.localize.term('treeHeaders_contentBlueprints');
		}

		// TODO: render the info from this instance in the list of actions
		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: {
				icon: 'icon-folder',
				label: this.localize.term('create_newFolder'),
				folderRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
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

	async #onCreateBlueprintClick(event: PointerEvent) {
		event.stopPropagation();
		const value = await umbOpenModal(this, UMB_DOCUMENT_TYPE_PICKER_MODAL, {
			data: {
				hideTreeRoot: true,
				pickableFilter: (item: UmbDocumentTypeTreeItemModel) => item.isElement == false,
			},
		});

		const selection = value.selection.filter((x) => x !== null);
		this.value = { documentTypeUnique: selection[0] };
		this.modalContext?.dispatchEvent(new UmbSelectionChangeEvent());
		this._submitModal();
	}

	override render() {
		return html`
			<uui-dialog-layout headline=${this.localize.term('actions_createblueprint')}>
				<uui-ref-list>
					<umb-ref-item
						name="New Document Blueprint for..."
						icon="icon-blueprint"
						@open=${this.#onCreateBlueprintClick}></umb-ref-item>

					<umb-ref-item
						name=${this.localize.term('create_newFolder') + '...'}
						icon="icon-folder"
						@open=${this.#onCreateFolderClick}></umb-ref-item>
				</uui-ref-list>

				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-box:first-child {
				margin-bottom: var(--uui-size-6);
			}
		`,
	];
}

export default UmbDocumentBlueprintOptionsCreateModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-create-options-modal': UmbDocumentBlueprintOptionsCreateModalElement;
	}
}
