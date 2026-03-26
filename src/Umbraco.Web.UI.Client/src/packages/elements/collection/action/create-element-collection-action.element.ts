import { UmbElementTypeStructureRepository } from '../../repository/structure/index.js';
import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import type { UmbAllowedElementTypeModel } from '../../repository/structure/index.js';
import type { UmbElementEntityTypeUnion } from '../../entity.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	map,
	nothing,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
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
	private _hrefList: Array<string | undefined> = [];

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
				const hrefPromises = this._createOptionControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
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
			parentEntityType: (this._parentEntityType ?? UMB_ELEMENT_ROOT_ENTITY_TYPE) as UmbElementEntityTypeUnion,
			parentUnique: this._parentUnique ?? 'null',
			documentTypeUnique: item.unique,
		});
	}

	async #onClick(
		event: Event,
		controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>,
		href?: string,
	) {
		if (href) return;
		event.stopPropagation();
		if (!controller.api) throw new Error('No API found');
		await controller.api.execute().catch(() => {});
	}

	#getTarget(href?: string) {
		if (href && href.startsWith('http')) {
			return '_blank';
		}
		return '_self';
	}

	override render() {
		if (this._allowedElementTypes.length === 0 && this._createOptionControllers.length === 0) return nothing;

		if (this._allowedElementTypes.length === 1 && this._createOptionControllers.length === 0) {
			return this.#renderCreateButton();
		}

		return this.#renderDropdown();
	}

	#renderCreateButton() {
		const item = this._allowedElementTypes[0];
		const label = `${this.localize.string(this.manifest?.meta.label || '#general_create')} ${item.name}`;
		return html`
			<uui-button color="default" look="outline" label=${label} href=${this.#getCreateUrl(item)}></uui-button>
		`;
	}

	#renderDropdown() {
		const label = this.localize.string(this.manifest?.meta.label || '#general_create');
		return html`
			<uui-button color="default" look="outline" label=${label} popovertarget="collection-action-menu-popover">
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
									<umb-icon slot="icon" name=${item.icon || 'icon-document'}></umb-icon>
								</uui-menu-item>
							`,
						)}
						${map(this._createOptionControllers, (controller, index) =>
							this.#renderCreateOptionItem(controller, index),
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderCreateOptionItem(controller: UmbExtensionApiInitializer<ManifestEntityCreateOptionAction>, index: number) {
		const manifest = controller.manifest;
		if (!manifest) return nothing;

		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
		const href = this._hrefList[index];

		return html`
			<uui-menu-item
				label=${manifest.meta.additionalOptions ? label + '...' : label}
				href=${ifDefined(href)}
				target=${this.#getTarget(href)}
				@click=${(event: Event) => this.#onClick(event, controller, href)}>
				<umb-icon slot="icon" name=${manifest.meta.icon || 'icon-folder'}></umb-icon>
			</uui-menu-item>
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

export default UmbCreateElementCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-element-collection-action': UmbCreateElementCollectionActionElement;
	}
}
