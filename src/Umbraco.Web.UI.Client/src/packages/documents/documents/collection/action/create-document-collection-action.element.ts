import { html, customElement, property, state, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbAllowedDocumentTypeModel } from '@umbraco-cms/backoffice/document-type';
import { UMB_WORKSPACE_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

@customElement('umb-create-document-collection-action')
export class UmbCreateDocumentCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedDocumentTypes: Array<UmbAllowedDocumentTypeModel> = [];

	@state()
	private _createDocumentPath = '';

	@state()
	private _documentUnique?: string;

	@state()
	private _documentTypeUnique?: string;

	@state()
	private _popoverOpen = false;

	@state()
	private _useInfiniteEditor = false;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#documentTypeStructureRepository = new UmbDocumentTypeStructureRepository(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('document')
			.onSetup(() => {
				return { data: { entityType: 'document', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._createDocumentPath = routeBuilder({});
			});

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext.unique, (unique) => {
				this._documentUnique = unique;
			});
			this.observe(workspaceContext.contentTypeUnique, (documentTypeUnique) => {
				this._documentTypeUnique = documentTypeUnique;
			});
		});

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (collectionContext) => {
			this.observe(collectionContext.filter, (filter) => {
				this._useInfiniteEditor = filter.useInfiniteEditor == true;
			});
		});
	}

	async firstUpdated() {
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

	// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedDocumentTypeModel) {
		// TODO: [LK] I need help with this. I don't know what the infinity editor URL should be.
		return this._useInfiniteEditor
			? `${this._createDocumentPath}create/${this._documentUnique ?? 'null'}/${item.unique}`
			: `section/content/workspace/document/create/${this._documentUnique ?? 'null'}/${item.unique}`;
	}

	render() {
		return this._allowedDocumentTypes.length !== 1 ? this.#renderDropdown() : this.#renderCreateButton();
	}

	#renderCreateButton() {
		if (this._allowedDocumentTypes.length !== 1) return;

		const item = this._allowedDocumentTypes[0];
		const label = (this.manifest?.meta.label ?? this.localize.term('general_create')) + ' ' + item.name;

		return html`<uui-button
			color="default"
			href=${this.#getCreateUrl(item)}
			label=${label}
			look="outline"></uui-button>`;
	}

	#renderDropdown() {
		if (!this._allowedDocumentTypes.length) return;

		const label = this.manifest?.meta.label ?? this.localize.term('general_create');

		return html`
			<uui-button popovertarget="collection-action-menu-popover" label=${label}>
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
}

export default UmbCreateDocumentCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-document-collection-action': UmbCreateDocumentCollectionActionElement;
	}
}
