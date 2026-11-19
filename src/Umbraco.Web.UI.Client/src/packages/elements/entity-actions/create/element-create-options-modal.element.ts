import { UmbElementTypeStructureRepository } from '../../repository/structure/index.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import {
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_CREATE,
} from '../../user-permissions/constants.js';
import type { UmbAllowedElementTypeModel } from '../../repository/structure/index.js';
import type { UmbElementEntityTypeUnion } from '../../entity.js';
import type {
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue,
} from './element-create-options-modal.token.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { createExtensionApiByAlias, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-element-create-options-modal')
export class UmbElementCreateOptionsModalElement extends UmbModalBaseElement<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
> {
	#elementTypeStructureRepository = new UmbElementTypeStructureRepository(this);

	#createPermitted = false;

	@state()
	private _allowedElementTypes: Array<UmbAllowedElementTypeModel> = [];

	@state()
	private _createOptionControllers: Array<UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>> = [];

	@state()
	private _hrefList: Array<string | undefined> = [];

	@state()
	private _search = '';

	constructor() {
		super();
		createExtensionApiByAlias(this, UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					alias: UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
					allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE],
				},
				onChange: (permitted: boolean) => {
					this.#createPermitted = permitted;
					if (permitted && this._allowedElementTypes.length === 0) {
						this.#retrieveAllowedElementTypes();
					}
				},
			},
		]);
	}

	override async firstUpdated() {
		this.#initCreateOptionActions();
	}

	async #retrieveAllowedElementTypes() {
		const parentUnique = this.data?.parent?.unique ?? null;
		const { data } = await this.#elementTypeStructureRepository.requestAllowedChildrenOf(null, parentUnique);

		if (data && this.#createPermitted) {
			this._allowedElementTypes = data.items;
		}
	}

	#initCreateOptionActions() {
		const parent = this.data?.parent;
		if (!parent) return;

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestEntityCreateOptionAction) => {
				return [{ entityType: parent.entityType, unique: parent.unique, meta: manifest.meta }];
			},
			(manifest: ManifestEntityCreateOptionAction) => manifest.forEntityTypes.includes(parent.entityType),
			async (controllers) => {
				this._createOptionControllers = controllers as unknown as Array<
					UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>
				>;
				const hrefPromises = this._createOptionControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
			},
			'umbElementCreateOptionActions',
		);
	}

	#onNavigate(elementType: UmbAllowedElementTypeModel) {
		if (!this.data) return;
		const parentEntityType = this.data.parent.entityType as UmbElementEntityTypeUnion;
		const parentUnique = this.data.parent.unique ?? null;
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

	async #onCreateOptionSelect(controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>, href?: string) {
		if (href) {
			history.pushState(null, '', href);
			this._submitModal();
			return;
		}

		if (!controller.api) throw new Error('No API found');

		try {
			await controller.api.execute();
			this._submitModal();
		} catch {
			// Keep the modal open on failure so the user can retry or choose another option.
		}
	}

	#matchesSearch(...texts: Array<string | undefined>) {
		const search = this._search.trim().toLowerCase();
		return !search || texts.some((text) => text?.toLowerCase().includes(search));
	}

	#getCreateOptionLabel(manifest?: ManifestEntityCreateOptionAction) {
		if (!manifest) return '';
		return manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
	}

	#getFilteredCreateOptions() {
		return this._createOptionControllers
			.map((controller, index) => ({ controller, href: this._hrefList[index] }))
			.filter(
				({ controller }) => controller.manifest && this.#matchesSearch(this.#getCreateOptionLabel(controller.manifest)),
			);
	}

	#getFilteredElementTypes() {
		return this._allowedElementTypes.filter((elementType) =>
			this.#matchesSearch(this.localize.string(elementType.name), this.localize.string(elementType.description ?? '')),
		);
	}

	#onSearch(event: UUIInputEvent) {
		this._search = event.target.value as string;
	}

	// Enter picks the first match: the first create option (for example a folder) while the search is empty,
	// otherwise the first element type.
	#onSearchKeydown(event: KeyboardEvent) {
		if (event.key !== 'Enter') return;
		event.preventDefault();

		const createOptions = this.#getFilteredCreateOptions();
		const [firstElementType] = this.#getFilteredElementTypes();
		if (createOptions.length && (!this._search.trim() || !firstElementType)) {
			this.#onCreateOptionSelect(createOptions[0].controller, createOptions[0].href);
		} else if (firstElementType) {
			this.#onNavigate(firstElementType);
		}
	}

	override render() {
		const optionCount = this._allowedElementTypes.length + this._createOptionControllers.length;
		return html`
			<uui-dialog-layout headline=${this.localize.term('general_create')}>
				${when(
					optionCount === 0,
					() => this.#renderNoAllowedTypes(),
					() => html`
						${when(optionCount > 8, () => this.#renderSearch())}
						<div id="options">${this.#renderOptions()}</div>
					`,
				)}
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('general_cancel')}
					@click="${this._rejectModal}"></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderSearch() {
		return html`
			<uui-input
				id="search"
				name="element-type-search"
				label=${this.localize.term('general_search')}
				placeholder=${this.localize.term('placeholders_search')}
				.value=${this._search}
				@input=${this.#onSearch}
				@keydown=${this.#onSearchKeydown}
				${umbFocus()}>
				<uui-icon name="icon-search" slot="prepend"></uui-icon>
			</uui-input>
		`;
	}

	#renderOptions() {
		const createOptions = this.#getFilteredCreateOptions();
		const elementTypes = this.#getFilteredElementTypes();
		if (createOptions.length === 0 && elementTypes.length === 0) {
			return html`<p id="no-results">${this.localize.term('general_noResultsFor', this._search.trim())}</p>`;
		}
		return html`
			<uui-ref-list>
				${this.#renderCreateOptionActions(createOptions)}${this.#renderAllowedElementTypes(elementTypes)}
			</uui-ref-list>
		`;
	}

	#renderNoAllowedTypes() {
		return html`
			<umb-localize key="create_noElementTypes">
				There are no allowed Element Types available for creating elements here. You must enable these in
				<strong>Document Types</strong> within the <strong>Settings</strong> section, by editing the
				<strong>Allow in Library</strong> under <strong>Structure</strong>.
			</umb-localize>
		`;
	}

	#renderAllowedElementTypes(elementTypes: Array<UmbAllowedElementTypeModel>) {
		return repeat(
			elementTypes,
			(elementType) => elementType.unique,
			(elementType) => html`
				<uui-ref-node-document-type
					.name=${this.localize.string(elementType.name)}
					.alias=${this.localize.string(elementType.description ?? '')}
					select-only
					selectable
					@selected=${() => this.#onNavigate(elementType)}>
					<umb-icon slot="icon" name=${elementType.icon ?? 'icon-document'}></umb-icon>
				</uui-ref-node-document-type>
			`,
		);
	}

	#renderCreateOptionActions(
		createOptions: Array<{
			controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>;
			href?: string;
		}>,
	) {
		return repeat(
			createOptions,
			({ controller }) => controller.manifest?.alias,
			({ controller, href }) => {
				const manifest = controller.manifest;
				if (!manifest) return nothing;

				return html`
					<uui-ref-node-document-type
						.name=${this.#getCreateOptionLabel(manifest)}
						select-only
						selectable
						@selected=${() => this.#onCreateOptionSelect(controller, href)}>
						<umb-icon slot="icon" name=${manifest.meta.icon}></umb-icon>
					</uui-ref-node-document-type>
				`;
			},
		);
	}

	static override styles = css`
		:host {
			display: block;
			/* A fixed width, so the dialog doesn't narrow as the search filters out the longer descriptions. */
			width: 800px;
			max-width: 100%;
		}

		/* Cap the dialog at 80% of the window and scroll only the options. Otherwise the modal's <dialog> scrolls
		   itself and its scrollbar squares off the rounded corners. The layout's slots are display: contents, so
		   the search, the options and the actions are its flex items. */
		uui-dialog-layout {
			display: flex;
			flex-direction: column;
			box-sizing: border-box;
			max-height: 80vh;
		}

		#search {
			flex: none;
			width: 100%;
			margin-bottom: var(--uui-size-space-4);
		}

		/* Each option's selection border sits 2px outside it. The padding keeps it inside the scroll box, so a short
		   list doesn't scroll and the focus ring isn't clipped; the negative margin keeps the options aligned. */
		#options {
			flex: 1 1 auto;
			min-height: 0;
			padding: 2px;
			margin: 0 -2px;
			overflow-x: hidden;
			overflow-y: auto;
		}

		#no-results {
			margin: var(--uui-size-space-4) 0;
			color: var(--uui-color-text-alt);
		}

		uui-ref-node-document-type {
			padding-top: var(--uui-size-3);
			padding-bottom: var(--uui-size-3);
		}
	`;
}

export default UmbElementCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-element-create-options-modal': UmbElementCreateOptionsModalElement;
	}
}
