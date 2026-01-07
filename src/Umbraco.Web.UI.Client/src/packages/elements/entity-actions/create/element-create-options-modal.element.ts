import { UmbElementItemRepository } from '../../item/index.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import type { UmbElementEntityTypeUnion } from '../../entity.js';
import type {
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue,
} from './element-create-options-modal.token.js';
import { html, customElement, state, repeat, css, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentTypeStructureRepository,
	type UmbAllowedDocumentTypeModel,
} from '@umbraco-cms/backoffice/document-type';

import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-element-create-options-modal')
export class UmbElementCreateOptionsModalElement extends UmbModalBaseElement<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
> {
	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);
	#elementItemRepository = new UmbElementItemRepository(this);

	@state()
	private _allowedElementTypes: UmbAllowedDocumentTypeModel[] = [];

	@state()
	private _headline: string =
		`${this.localize.term('create_createUnder')} ${this.localize.term('actionCategories_content')}`;

	override async firstUpdated() {
		const parentUnique = this.data?.parent.unique;
		const documentTypeUnique = this.data?.documentType?.unique || null;

		this.#retrieveAllowedElementTypesOf(documentTypeUnique, parentUnique || null);

		if (parentUnique) {
			this.#retrieveHeadline(parentUnique);
		}
	}

	async #retrieveAllowedElementTypesOf(unique: string | null, parentContentUnique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique, parentContentUnique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedElementTypes = data.items;
		}
	}

	async #retrieveHeadline(parentUnique: string) {
		if (!parentUnique) return;
		const { data } = await this.#elementItemRepository.requestItems([parentUnique]);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = `${this.localize.term('create_createUnder')} ${data[0].variants?.[0].name ?? this.localize.term('actionCategories_content')}`;
		}
	}

	// close the modal when navigating to data type
	#onNavigate(documentTypeUnique: string) {
		if (!this.data) {
			throw new Error('Data is not defined');
		}

		history.pushState(
			null,
			'',
			UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
				parentEntityType: this.data.parent.entityType as UmbElementEntityTypeUnion,
				parentUnique: this.data.parent.unique,
				documentTypeUnique,
			}),
		);

		this._submitModal();
	}

	async #onSelectElementType(documentTypeUnique: UmbEntityUnique) {
		if (!documentTypeUnique) {
			throw new Error('Element type unique is not defined');
		}

		this.#onNavigate(documentTypeUnique);
		return;
	}

	override render() {
		const headline = this._headline;
		return html`
			<uui-dialog-layout headline=${headline}>
				${this.#renderElementTypes()}
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderNoElementTypes() {
		if (this.data?.documentType?.unique) {
			return html`
				<umb-localize key="create_noDocumentTypes">
					There are no allowed Document Types available for creating content here. You must enable these in
					<strong>Document Types</strong> within the <strong>Settings</strong> section, by editing the
					<strong>Allowed child node types</strong> under <strong>Structure</strong>.
				</umb-localize>
				<br />
				<uui-button
					id="edit-permissions"
					look="secondary"
					href=${`/section/settings/workspace/document-type/edit/${this.data?.documentType?.unique}/view/structure`}
					label=${this.localize.term('create_noElementTypesEditPermissions')}
					@click=${() => this._rejectModal()}></uui-button>
			`;
		} else {
			return html`
				<umb-localize key="create_noDocumentTypesAllowedAtRoot">
					There are no allowed Document Types available for creating content here. You must enable these in
					<strong>Document Types</strong> within the <strong>Settings</strong> section, by changing the
					<strong>Allow as root</strong> option under <strong>Structure</strong>.
				</umb-localize>
			`;
		}
	}

	#renderElementTypes() {
		return when(
			this._allowedElementTypes.length === 0,
			() => this.#renderNoElementTypes(),
			() =>
				repeat(
					this._allowedElementTypes,
					(documentType) => documentType.unique,
					(documentType) => html`
						<uui-ref-node-document-type
							.name=${this.localize.string(documentType.name)}
							.alias=${this.localize.string(documentType.description ?? '')}
							select-only
							selectable
							@selected=${() => this.#onSelectElementType(documentType.unique)}
							@open=${() => this.#onSelectElementType(documentType.unique)}>
							<umb-icon slot="icon" name=${documentType.icon || 'icon-circle-dotted'}></umb-icon>
						</uui-ref-node-document-type>
					`,
				),
		);
	}

	static override styles = [
		UmbTextStyles,
		css`
			#blank {
				border-bottom: 1px solid var(--uui-color-border);
			}

			#edit-permissions {
				margin-top: var(--uui-size-6);
			}
		`,
	];
}

export default UmbElementCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-create-options-modal': UmbElementCreateOptionsModalElement;
	}
}
