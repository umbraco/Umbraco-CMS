import type { UmbElementEntityTypeUnion } from '../../entity.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import {
	UmbElementTypeStructureRepository,
	type UmbAllowedElementTypeModel,
} from '../../repository/structure/index.js';
import { html, customElement, property, state, map, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-create-element-collection-action')
export class UmbCreateElementCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedElementTypes: Array<UmbAllowedElementTypeModel> = [];

	@state()
	private _createOptionControllers: Array<UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>> = [];

	@state()
	private _parentUnique?: UmbEntityUnique;

	@state()
	private _parentEntityType?: string;

	@state()
	private _popoverOpen = false;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#elementTypeStructureRepository = new UmbElementTypeStructureRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (entityContext) => {
			if (!entityContext) return;
			this._parentUnique = entityContext.getUnique();
			this._parentEntityType = entityContext.getEntityType();
			this.#initCreateOptionActions();
		});
	}

	override async firstUpdated() {
		const { data } = await this.#elementTypeStructureRepository.requestAllowedChildrenOf(null, null);
		if (data?.items) {
			this._allowedElementTypes = data.items;
		}
	}

	#initCreateOptionActions() {
		const entityType = this._parentEntityType;
		if (!entityType) return;

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestEntityCreateOptionAction) => {
				return [{ entityType, unique: this._parentUnique ?? null, meta: manifest.meta }];
			},
			(manifest: ManifestEntityCreateOptionAction) => manifest.forEntityTypes.includes(entityType),
			async (controllers) => {
				this._createOptionControllers = controllers as unknown as Array<
					UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>
				>;
			},
			'umbCollectionCreateOptionActions',
		);
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedElementTypeModel) {
		if (!item.unique) {
			throw new Error('Item does not have a unique identifier');
		}
		return UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType: (this._parentEntityType ?? 'element-root') as UmbElementEntityTypeUnion,
			parentUnique: this._parentUnique ?? 'null',
			documentTypeUnique: item.unique,
		});
	}

	async #onExecuteCreateOption(controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>) {
		await controller.api?.execute();
	}

	get #totalOptions() {
		return this._allowedElementTypes.length + this._createOptionControllers.length;
	}

	override render() {
		if (this.#totalOptions === 0) return nothing;

		if (this._allowedElementTypes.length === 1 && this._createOptionControllers.length === 0) {
			return this.#renderCreateButton();
		}

		return this.#renderDropdown();
	}

	#renderCreateButton() {
		const item = this._allowedElementTypes[0];
		const label =
			(this.manifest?.meta.label
				? this.localize.string(this.manifest?.meta.label)
				: this.localize.term('general_create')) +
			' ' +
			item.name;

		return html`<uui-button
			color="default"
			href=${this.#getCreateUrl(item)}
			label=${label}
			look="outline"></uui-button>`;
	}

	#renderDropdown() {
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
							this._allowedElementTypes,
							(item) => html`
								<uui-menu-item label="${item.name}..." href=${this.#getCreateUrl(item)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-document'}></umb-icon>
								</uui-menu-item>
							`,
						)}
						${map(this._createOptionControllers, (controller) => this.#renderCreateOptionItem(controller))}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderCreateOptionItem(controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>) {
		const manifest = controller.manifest;
		if (!manifest) return nothing;

		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;

		return html`
			<uui-menu-item
				label=${manifest.meta.additionalOptions ? label + '...' : label}
				@click=${() => this.#onExecuteCreateOption(controller)}>
				<umb-icon slot="icon" name=${manifest.meta.icon ?? 'icon-folder'}></umb-icon>
			</uui-menu-item>
		`;
	}
}

export default UmbCreateElementCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-element-collection-action': UmbCreateElementCollectionActionElement;
	}
}
