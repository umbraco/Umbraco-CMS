import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../workspace/constants.js';
import { UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_DOCUMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_CREATE_OPTIONS_MODAL } from '../../entity-actions/create/constants.js';
import { customElement, html, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbDocumentBlueprintItemRepository } from '@umbraco-cms/backoffice/document-blueprint';
import { UmbPopoverScrollElement } from '@umbraco-cms/backoffice/collection';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';
import type { UmbAllowedDocumentTypeModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

@customElement('umb-create-document-collection-action')
export class UmbCreateDocumentCollectionActionElement extends UmbPopoverScrollElement {
	@state()
	private _allowedDocumentTypes: Array<UmbAllowedDocumentTypeModel> = [];

	@state()
	private _documentUnique?: UmbEntityUnique;

	@state()
	private _documentTypeUnique?: string;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);
	#documentBlueprintItemRepository = new UmbDocumentBlueprintItemRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext?.unique, (unique) => {
				this._documentUnique = unique;
			});
			this.observe(workspaceContext?.contentTypeUnique, (documentTypeUnique) => {
				this._documentTypeUnique = documentTypeUnique;
			});
		});
	}

	override async firstUpdated() {
		if (this._documentTypeUnique) {
			this.#retrieveAllowedDocumentTypesOf(this._documentTypeUnique, this._documentUnique || null);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null, parentContentUnique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique, parentContentUnique);

		if (data && data.items) {
			this._allowedDocumentTypes = data.items;
		}
	}

	#getCreateUrl(item: UmbAllowedDocumentTypeModel) {
		if (!item.unique) {
			throw new Error('Item unique is missing');
		}
		return UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType: this._documentUnique ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
			parentUnique: this._documentUnique ?? 'null',
			documentTypeUnique: item.unique,
		});
	}

	#onClick(event: MouseEvent, item: UmbAllowedDocumentTypeModel) {
		// Let modified/non-primary clicks (open in new tab, new window, etc.) use native link navigation.
		if (event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
			return;
		}
		// Cancel native anchor navigation and stop the window-level router-slot click handler
		// (see core/router/router-slot/util/anchor.ts) from SPA-navigating before we can decide
		// whether to open the blueprint picker.
		event.preventDefault();
		event.stopPropagation();
		this.#onSelect(item);
	}

	async #onSelect(item: UmbAllowedDocumentTypeModel) {
		if (!item.unique) {
			throw new Error('Item unique is missing');
		}

		const createUrl = this.#getCreateUrl(item);

		const { data } = await this.#documentBlueprintItemRepository.requestItemsByDocumentType(item.unique);

		if (!data?.length) {
			history.pushState(null, '', createUrl);
			return;
		}

		await umbOpenModal(this, UMB_DOCUMENT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this._documentUnique ?? null,
					entityType: this._documentUnique ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
				},
				documentType: this._documentTypeUnique ? { unique: this._documentTypeUnique } : null,
				preselectedDocumentType: {
					unique: item.unique,
					icon: item.icon ?? undefined,
					blueprints: data,
				},
			},
		});
	}

	override render() {
		return this._allowedDocumentTypes.length !== 1 ? this.#renderDropdown() : this.#renderCreateButton();
	}

	#renderCreateButton() {
		if (this._allowedDocumentTypes.length !== 1) return;

		const item = this._allowedDocumentTypes[0];
		// TODO: Stop appending values to labels, instead we need to parse the name as a argument to the label. [NL]
		const label =
			(this.manifest?.meta.label
				? this.localize.string(this.manifest?.meta.label)
				: this.localize.term('general_create')) +
			' ' +
			this.localize.string(item.name);

		return html`
			<uui-button
				color="default"
				href=${this.#getCreateUrl(item)}
				label=${label}
				look="outline"
				@click=${(event: MouseEvent) => this.#onClick(event, item)}></uui-button>
		`;
	}

	#renderDropdown() {
		if (!this._allowedDocumentTypes.length) return;

		const label = this.manifest?.meta.label
			? this.localize.string(this.manifest?.meta.label)
			: this.localize.term('general_create');

		return html`
			<uui-button popovertarget="collection-action-menu-popover" label=${label} color="default" look="outline">
				${label}
				<uui-symbol-expand .open=${this.popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this._onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${repeat(
							this._allowedDocumentTypes,
							(item) => item.unique,
							(item) => html`
								<uui-menu-item
									label=${this.localize.string(item.name)}
									href=${this.#getCreateUrl(item)}
									@click=${(event: MouseEvent) => this.#onClick(event, item)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-document'}></umb-icon>
								</uui-menu-item>
							`,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}
}

export default UmbCreateDocumentCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-collection-action': UmbCreateDocumentCollectionActionElement;
	}
}
