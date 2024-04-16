import { UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from '../../../tree/index.js';
import type { UmbDocumentTypeCreateOptionsModalData } from './index.js';
import { html, customElement, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbCreateFolderEntityAction } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-type-create-options-modal')
export class UmbDataTypeCreateOptionsModalElement extends UmbModalBaseElement<UmbDocumentTypeCreateOptionsModalData> {
	#createFolderAction?: UmbCreateFolderEntityAction;

	#items: Array<{ preset: string; label: string; description: string; icon: string }> = [
		{
			preset: 'null',
			label: this.localize.term('create_documentType'),
			description: this.localize.term('create_documentTypeDescription'),
			icon: 'icon-document',
		},
		{
			preset: 'template',
			label: this.localize.term('create_documentTypeWithTemplate'),
			description: this.localize.term('create_documentTypeWithTemplateDescription'),
			icon: 'icon-notepad',
		},
		{
			preset: 'element',
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
			preset: 'folder',
			label: this.localize.term('create_folder'),
			description: this.localize.term('create_folderDescription'),
			icon: 'icon-folder',
		},
	];

	connectedCallback(): void {
		super.connectedCallback();

		if (!this.data?.parent) throw new Error('A parent is required to create a folder');

		this.#createFolderAction = new UmbCreateFolderEntityAction(this, {
			unique: this.data.parent.unique,
			entityType: this.data.parent.entityType,
			meta: { icon: '', label: '', folderRepositoryAlias: UMB_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS },
		});
	}
	async #onClick(preset: string) {
		switch (preset) {
			case 'folder': {
				try {
					await this.#createFolderAction?.execute();
					this._submitModal();
				} catch (error) {
					//console.error(error);
				}

				break;
			}

			default: {
				const entityType = this.data?.parent.entityType;
				const unique = this.data?.parent.unique || 'null';
				const href = `section/settings/workspace/document-type/create/${entityType}/${unique}/${preset}`;
				window.history.pushState({}, '', href);

				this._submitModal();
				break;
			}
		}
	}

	render() {
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
									@click=${() => this.#onClick(item.preset)}></umb-ref-item>
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
