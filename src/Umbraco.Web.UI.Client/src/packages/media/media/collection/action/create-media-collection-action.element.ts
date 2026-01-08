import { UMB_MEDIA_COLLECTION_CONTEXT } from '../media-collection.context-token.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../constants.js';
import { UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { html, customElement, property, state, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/collection';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';

@customElement('umb-create-media-collection-action')
export class UmbCreateMediaCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];

	@state()
	private _workspacePathBuilder?: UmbModalRouteBuilder;

	@state()
	private _mediaUnique?: UmbEntityUnique;

	@state()
	private _mediaTypeUnique?: string;

	@state()
	private _popoverOpen = false;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext.unique, (unique) => {
				this._mediaUnique = unique;
			});

			this.observe(workspaceContext.contentTypeUnique, (mediaTypeUnique) => {
				this._mediaTypeUnique = mediaTypeUnique;
			});
		});

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.observe(collectionContext.workspacePathBuilder, (builder) => {
				this._workspacePathBuilder = builder;
			});
		});
	}

	override async firstUpdated() {
		this.#retrieveAllowedMediaTypesOf(this._mediaTypeUnique ?? '', this._mediaUnique || null);
	}

	async #retrieveAllowedMediaTypesOf(unique: string | null, parentContentUnique: string | null) {
		const { data } = await this.#mediaTypeStructureRepository.requestAllowedChildrenOf(unique, parentContentUnique);
		if (data && data.items) {
			this._allowedMediaTypes = data.items;
		}
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedMediaTypeModel) {
		return item.unique && this._workspacePathBuilder
			? this._workspacePathBuilder({ entityType: UMB_MEDIA_ENTITY_TYPE }) +
					UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({
						parentEntityType: this._mediaUnique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
						parentUnique: this._mediaUnique ?? 'null',
						mediaTypeUnique: item.unique,
					})
			: '';
	}

	override render() {
		return this._allowedMediaTypes.length !== 1 ? this.#renderDropdown() : this.#renderCreateButton();
	}

	#renderCreateButton() {
		if (this._allowedMediaTypes.length !== 1) return;

		const item = this._allowedMediaTypes[0];
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
		if (!this._allowedMediaTypes.length) return;

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
							this._allowedMediaTypes,
							(item) => html`
								<uui-menu-item label=${item.name} href=${this.#getCreateUrl(item)}>
									<umb-icon slot="icon" name=${item.icon ?? 'icon-picture'}></umb-icon>
								</uui-menu-item>
							`,
						)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}
}

export default UmbCreateMediaCollectionActionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-media-collection-action': UmbCreateMediaCollectionActionElement;
	}
}
