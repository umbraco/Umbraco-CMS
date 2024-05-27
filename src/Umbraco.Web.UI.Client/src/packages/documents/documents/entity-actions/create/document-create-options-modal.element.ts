import { UmbDocumentItemRepository } from '../../repository/index.js';
import type {
	UmbDocumentCreateOptionsModalData,
	UmbDocumentCreateOptionsModalValue,
} from './document-create-options-modal.token.js';
import { html, customElement, state, ifDefined, repeat, css, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import {
	UmbDocumentTypeStructureRepository,
	type UmbAllowedDocumentTypeModel,
} from '@umbraco-cms/backoffice/document-type';
import {
	UmbDocumentBlueprintItemRepository,
	type UmbDocumentBlueprintItemBaseModel,
} from '@umbraco-cms/backoffice/document-blueprint';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN,
	type UmbDocumentEntityTypeUnion,
} from '@umbraco-cms/backoffice/document';

@customElement('umb-document-create-options-modal')
export class UmbDocumentCreateOptionsModalElement extends UmbModalBaseElement<
	UmbDocumentCreateOptionsModalData,
	UmbDocumentCreateOptionsModalValue
> {
	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);
	#documentItemRepository = new UmbDocumentItemRepository(this);
	#documentBlueprintItemRepository = new UmbDocumentBlueprintItemRepository(this);

	#documentTypeUnique = '';
	#documentTypeIcon = '';

	@state()
	private _allowedDocumentTypes: UmbAllowedDocumentTypeModel[] = [];

	@state()
	private _headline: string =
		`${this.localize.term('create_createUnder')} ${this.localize.term('actionCategories_content')}`;

	@state()
	private _availableBlueprints: Array<UmbDocumentBlueprintItemBaseModel> = [];

	async firstUpdated() {
		const parentUnique = this.data?.parent.unique;
		const documentTypeUnique = this.data?.documentType?.unique || null;

		this.#retrieveAllowedDocumentTypesOf(documentTypeUnique);

		if (parentUnique) {
			this.#retrieveHeadline(parentUnique);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique);

		if (data) {
			// TODO: implement pagination, or get 1000?
			this._allowedDocumentTypes = data.items;
		}
	}

	async #retrieveHeadline(parentUnique: string) {
		if (!parentUnique) return;
		const { data } = await this.#documentItemRepository.requestItems([parentUnique]);
		if (data) {
			// TODO: we need to get the correct variant context here
			this._headline = `${this.localize.term('create_createUnder')} ${data[0].variants?.[0].name ?? this.localize.term('actionCategories_content')}`;
		}
	}

	// close the modal when navigating to data type
	#onNavigate(documentTypeUnique: string, blueprintUnique?: string) {
		if (!this.data) {
			throw new Error('Data is not defined');
		}
		if (!blueprintUnique) {
			history.pushState(
				null,
				'',
				UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
					parentEntityType: this.data.parent.entityType as UmbDocumentEntityTypeUnion,
					parentUnique: this.data.parent.unique,
					documentTypeUnique,
				}),
			);
		} else {
			history.pushState(
				null,
				'',
				UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
					parentEntityType: this.data.parent.entityType as UmbDocumentEntityTypeUnion,
					parentUnique: this.data.parent.unique,
					documentTypeUnique,
					blueprintUnique,
				}),
			);
		}
		this._submitModal();
	}

	async #onSelectDocumentType(documentTypeUnique: string) {
		this.#documentTypeUnique = documentTypeUnique;
		this.#documentTypeIcon = this._allowedDocumentTypes.find((dt) => dt.unique === documentTypeUnique)?.icon ?? '';

		const { data } = await this.#documentBlueprintItemRepository.requestItemsByDocumentType(documentTypeUnique);

		this._availableBlueprints = data ?? [];

		if (!this._availableBlueprints.length) {
			this.#onNavigate(documentTypeUnique);
			return;
		}
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_create')}>
				${this._availableBlueprints.length && this.#documentTypeUnique
					? this.#renderBlueprints()
					: this.#renderDocumentTypes()}
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}

	#renderDocumentTypes() {
		return html`<uui-box .headline=${this._headline}>
			${when(
				this._allowedDocumentTypes.length === 0,
				() => html`
					<umb-localize key="create_noDocumentTypes">
						There are no allowed Document Types available for creating content here. You must enable these in
						<strong>Document Types</strong> within the <strong>Settings</strong> section, by editing the
						<strong>Allowed child node types</strong> under <strong>Permissions</strong>
					</umb-localize>
				`,
				() =>
					repeat(
						this._allowedDocumentTypes,
						(documentType) => documentType.unique,
						(documentType) =>
							html` <uui-ref-node-document-type
								data-id=${ifDefined(documentType.unique)}
								.name=${documentType.name}
								.alias=${documentType.description}
								select-only
								selectable
								@selected=${() => this.#onSelectDocumentType(documentType.unique)}>
								<umb-icon slot="icon" name=${documentType.icon || 'icon-circle-dotted'}></umb-icon>
							</uui-ref-node-document-type>`,
					),
			)}
		</uui-box>`;
	}

	#renderBlueprints() {
		return html`<uui-box headline=${this.localize.term('blueprints_selectBlueprint')}>
			<uui-menu-item
				id="blank"
				label=${this.localize.term('blueprints_blankBlueprint')}
				@click=${() => this.#onNavigate(this.#documentTypeUnique)}>
				<umb-icon slot="icon" name=${this.#documentTypeIcon}></umb-icon>
			</uui-menu-item>
			${repeat(
				this._availableBlueprints,
				(blueprint) => blueprint.unique,
				(blueprint) =>
					html`<uui-menu-item
						label=${blueprint.name}
						@click=${() => this.#onNavigate(this.#documentTypeUnique, blueprint.unique)}>
						<umb-icon slot="icon" name="icon-blueprint"></umb-icon>
					</uui-menu-item>`,
			)}</uui-box
		> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			#blank {
				border-bottom: 1px solid var(--uui-color-border);
			}
		`,
	];
}

export default UmbDocumentCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-create-options-modal': UmbDocumentCreateOptionsModalElement;
	}
}
