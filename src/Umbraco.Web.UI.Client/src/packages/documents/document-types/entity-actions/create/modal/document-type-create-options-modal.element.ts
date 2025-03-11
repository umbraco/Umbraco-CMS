import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/index.js';
import type { UmbDocumentTypeCreateOptionsModalData } from './constants.js';
import { customElement, html, map } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
} from '@umbraco-cms/backoffice/document-type';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type {
	UmbCreateDocumentTypeWorkspacePresetType,
	UmbDocumentTypeEntityTypeUnion,
} from '@umbraco-cms/backoffice/document-type';

const UMB_CREATE_FOLDER_PRESET = 'folder';

// Include the types from the DocumentTypeWorkspacePresetType + folder.
type OptionsPresetType = UmbCreateDocumentTypeWorkspacePresetType | typeof UMB_CREATE_FOLDER_PRESET | null;

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
@customElement('umb-document-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbModalBaseElement<UmbDocumentTypeCreateOptionsModalData> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	#items: Array<{
		preset: OptionsPresetType;
		label: string;
		description: string;
		icon: string;
	}> = [
		{
			preset: null,
			label: this.localize.term('create_documentType'),
			description: this.localize.term('create_documentTypeDescription'),
			icon: 'icon-document',
		},
		{
			preset: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
			label: this.localize.term('create_documentTypeWithTemplate'),
			description: this.localize.term('create_documentTypeWithTemplateDescription'),
			icon: 'icon-document-html',
		},
		{
			preset: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
			label: this.localize.term('create_elementType'),
			description: this.localize.term('create_elementTypeDescription'),
			icon: 'icon-plugin',
		},
		// TODO: Investigate alternative options to inform about compositions functionality. [LK/NL]
		// {
		// 	preset: 'composition',
		// 	label: this.localize.term('create_composition'),
		// 	description: this.localize.term('create_compositionDescription'),
		// 	icon: 'icon-plugin',
		// },
		{
			preset: UMB_CREATE_FOLDER_PRESET,
			label: this.localize.term('create_folder'),
			description: this.localize.term('create_folderDescription'),
			icon: 'icon-folder',
		},
	];

	override connectedCallback(): void {
		super.connectedCallback();

		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: { icon: '', label: '', folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS },
		});
	}

	async #onClick(presetAlias: OptionsPresetType) {
		switch (presetAlias) {
			case UMB_CREATE_FOLDER_PRESET: {
				try {
					await this.#createFolderAction?.execute();
					this._submitModal();
					return;
				} catch {
					//console.error(error);
				}

				break;
			}

			default: {
				const parentEntityType = this.data?.parent.entityType as UmbDocumentTypeEntityTypeUnion;
				if (!parentEntityType) throw new Error('Entity type is required to create a document type');
				const parentUnique = this.data?.parent.unique ?? null;
				const href = UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.generateAbsolute({
					parentEntityType,
					parentUnique,
					presetAlias,
				});
				window.history.pushState({}, '', href);

				this._submitModal();
				break;
			}
		}
	}

	override render() {
		return html`
			<umb-body-layout
				headline="${this.localize.term('create_createUnder')} ${this.localize.term('treeHeaders_documentTypes')}">
				<uui-box>
					<uui-ref-list>
						${map(
							this.#items,
							(item) => html`
								<umb-ref-item
									name=${item.label}
									detail=${item.description}
									icon=${item.icon}
									@open=${() => this.#onClick(item.preset)}></umb-ref-item>
							`,
						)}
					</uui-ref-list>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
			</umb-body-layout>
		`;
	}
}

export default UmbDataTypeCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-create-options-modal': UmbDataTypeCreateOptionsModalElement;
	}
}
