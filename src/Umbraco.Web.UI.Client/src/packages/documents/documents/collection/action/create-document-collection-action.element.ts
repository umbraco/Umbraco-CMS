import { UMB_DOCUMENT_COLLECTION_CONTEXT } from '../document-collection.context-token.js';
import { css, customElement, html, map, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import {
	UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN,
	UMB_DOCUMENT_ENTITY_TYPE,
	UMB_DOCUMENT_ROOT_ENTITY_TYPE,
	UMB_DOCUMENT_WORKSPACE_CONTEXT,
} from '@umbraco-cms/backoffice/document';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';
import type { UmbAllowedDocumentTypeModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

@customElement('umb-create-document-collection-action')
export class UmbCreateDocumentCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedDocumentTypes: Array<UmbAllowedDocumentTypeModel> = [];

	@state()
	private _workspacePathBuilder?: UmbModalRouteBuilder;

	@state()
	private _documentUnique?: UmbEntityUnique;

	@state()
	private _documentTypeUnique?: string;

	@state()
	private _popoverOpen = false;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext.unique, (unique) => {
				this._documentUnique = unique;
			});
			this.observe(workspaceContext.contentTypeUnique, (documentTypeUnique) => {
				this._documentTypeUnique = documentTypeUnique;
			});
		});

		this.consumeContext(UMB_DOCUMENT_COLLECTION_CONTEXT, (collectionContext) => {
			this.observe(collectionContext.workspacePathBuilder, (builder) => {
				this._workspacePathBuilder = builder;
			});
		});
	}

	override async firstUpdated() {
		if (this._documentTypeUnique) {
			this.#retrieveAllowedDocumentTypesOf(this._documentTypeUnique);
		}
	}

	async #retrieveAllowedDocumentTypesOf(unique: string | null) {
		const { data } = await this.#documentTypeStructureRepository.requestAllowedChildrenOf(unique);

		if (data && data.items) {
			this._allowedDocumentTypes = data.items;
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedDocumentTypeModel) {
		return item.unique && this._workspacePathBuilder
			? this._workspacePathBuilder({ entityType: UMB_DOCUMENT_ENTITY_TYPE }) +
					UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN.generateLocal({
						parentEntityType: this._documentUnique ? UMB_DOCUMENT_ENTITY_TYPE : UMB_DOCUMENT_ROOT_ENTITY_TYPE,
						parentUnique: this._documentUnique ?? 'null',
						documentTypeUnique: item.unique,
					})
			: '';
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
			item.name;

		return html`
			<uui-button color="default" href=${this.#getCreateUrl(item)} label=${label} look="outline"></uui-button>
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
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${map(
							this._allowedDocumentTypes,
							(item) => html`
								<uui-menu-item label=${item.name} href=${this.#getCreateUrl(item)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-document'}></umb-icon>
								</uui-menu-item>
							`,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			uui-scroll-container {
				max-height: 500px;
			}
		`,
	];
}

export default UmbCreateDocumentCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-collection-action': UmbCreateDocumentCollectionActionElement;
	}
}
