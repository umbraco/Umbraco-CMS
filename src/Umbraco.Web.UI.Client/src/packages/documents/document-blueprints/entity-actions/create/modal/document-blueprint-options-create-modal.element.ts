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
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { type UmbSelectedEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbCreateFolderEntityAction, type UmbTreeElement } from '@umbraco-cms/backoffice/tree';

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

		try {
			await this.#createFolderAction?.execute();
			this._submitModal();
		} catch (error) {
			return;
		}
	}

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		const element = event.target as UmbTreeElement;
		this.value = { documentTypeUnique: element.getSelection()[0] };
		this.modalContext?.dispatchEvent(new UmbSelectionChangeEvent());
		this._submitModal();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_createblueprint')}>
				<uui-box headline=${this.localize.term('blueprints_createBlueprintFolderUnder', this._parentName)}>
					<uui-menu-item @click=${this.#onCreateFolderClick} label=${this.localize.term('create_newFolder') + '...'}>
						<uui-icon slot="icon" name="icon-folder"></uui-icon>
					</uui-menu-item>
				</uui-box>
				<uui-box headline=${this.localize.term('blueprints_createBlueprintItemUnder', this._parentName)}>
					<umb-localize key="create_createContentBlueprint">
						Select the Document Type you want to make a Document Blueprint for
					</umb-localize>
					<umb-tree
						alias="Umb.Tree.DocumentType"
						.props=${{
							hideTreeRoot: true,
							selectableFilter: (item: any) => item.isElement == false,
						}}
						@selected=${this.#onSelected}></umb-tree>
				</uui-box>
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this._rejectModal}"></uui-button>
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
