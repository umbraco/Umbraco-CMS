import type { UmbElementEntityTypeUnion } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../../folder/repository/constants.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import {
	UmbElementTypeStructureRepository,
	type UmbAllowedElementTypeModel,
} from '../../repository/structure/index.js';
import type {
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue,
} from './element-create-options-modal.token.js';
import { html, nothing, customElement, state, repeat, css, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_FOLDER_CREATE_MODAL } from '@umbraco-cms/backoffice/tree';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-element-create-options-modal')
export class UmbElementCreateOptionsModalElement extends UmbModalBaseElement<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
> {
	#elementTypeStructureRepository = new UmbElementTypeStructureRepository(this);

	@state()
	private _allowedElementTypes: UmbAllowedElementTypeModel[] = [];

	@state()
	private _headline: string = this.localize.term('general_create');

	override async firstUpdated() {
		this.#retrieveAllowedElementTypes();
	}

	async #retrieveAllowedElementTypes() {
		const { data } = await this.#elementTypeStructureRepository.requestAllowedChildrenOf(null, null);

		if (data) {
			this._allowedElementTypes = data.items;
		}
	}

	#onNavigate(elementType: UmbAllowedElementTypeModel) {
		const parentEntityType = this.data?.parent.entityType as UmbElementEntityTypeUnion;
		const parentUnique = this.data?.parent.unique ?? null;
		const documentTypeUnique = elementType.unique;
		if (!documentTypeUnique) return;

		history.pushState(
			null,
			'',
			UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
				parentEntityType,
				parentUnique,
				documentTypeUnique,
			}),
		);
		this._submitModal();
	}

	async #onCreateFolder() {
		const parent = this.data?.parent;
		if (!parent) return;

		await umbOpenModal(this, UMB_FOLDER_CREATE_MODAL, {
			data: {
				folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
				parent: {
					unique: parent.unique,
					entityType: parent.entityType,
				},
			},
		});

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) return;
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: parent.entityType,
			unique: parent.unique,
		});
		eventContext.dispatchEvent(event);

		this._submitModal();
	}

	override render() {
		return html`
			<uui-dialog-layout headline=${this._headline ?? ''}>
				${when(
					this._allowedElementTypes.length === 0,
					() => this.#renderNoAllowedTypes(),
					() => this.#renderAllowedElementTypes(),
				)}
				${this.#renderFolderOption()}
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderNoAllowedTypes() {
		return html`
			<umb-localize key="create_noDocumentTypes">
				There are no allowed Element Types available for creating elements here. You must enable these in
				<strong>Document Types</strong> within the <strong>Settings</strong> section, by editing the
				<strong>Allow at library root</strong> under <strong>Permissions</strong>.
			</umb-localize>
		`;
	}

	#renderAllowedElementTypes() {
		return repeat(
			this._allowedElementTypes,
			(elementType) => elementType.unique,
			(elementType) => html`
				<uui-ref-node-document-type
					.name=${this.localize.string(elementType.name) + '...'}
					.alias=${this.localize.string(elementType.description ?? '')}
					select-only
					selectable
					@selected=${() => this.#onNavigate(elementType)}>
					${elementType.icon ? html`<umb-icon slot="icon" name=${elementType.icon}></umb-icon>` : nothing}
				</uui-ref-node-document-type>
			`,
		);
	}

	#renderFolderOption() {
		return html`
			<uui-ref-node-document-type
				.name=${this.localize.term('create_folder') + '...'}
				select-only
				selectable
				@selected=${() => this.#onCreateFolder()}>
				<umb-icon slot="icon" name="icon-folder"></umb-icon>
			</uui-ref-node-document-type>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbElementCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-create-options-modal': UmbElementCreateOptionsModalElement;
	}
}
