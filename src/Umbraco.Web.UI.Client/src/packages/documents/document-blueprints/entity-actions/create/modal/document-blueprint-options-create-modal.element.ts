import { UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS } from '../../../tree/folder/manifests.js';
import type {
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue,
} from './index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { type UmbSelectedEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbCreateFolderEntityAction, type UmbTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-blueprint-options-create-modal')
export class UmbDocumentBlueprintOptionsCreateModalElement extends UmbModalBaseElement<
	UmbDocumentBlueprintOptionsCreateModalData,
	UmbDocumentBlueprintOptionsCreateModalValue
> {
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
				folderRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
			},
		});
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

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		const element = event.target as UmbTreeElement;
		this.value = { documentTypeUnique: element.getSelection()[0] };
		this.modalContext?.dispatchEvent(new UmbSelectionChangeEvent());
		this._submitModal();
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_createblueprint')}>
				<uui-box headline="Create a folder under Content Templates">
					<uui-menu-item @click=${this.#onCreateFolderClick} label="New Folder...">
						<uui-icon slot="icon" name="icon-folder"></uui-icon>
					</uui-menu-item>
				</uui-box>
				<uui-box headline="Create an item under Content Templates">
					<umb-localize key="create_createContentBlueprint">
						Select the Document Type you want to make a content blueprint for
					</umb-localize>
					<umb-tree
						alias="Umb.Tree.DocumentType"
						.props=${{
							hideTreeRoot: true,
							selectableFilter: (item: any) => item.isElement == false,
						}}
						@selected=${this.#onSelected}></umb-tree>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
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
