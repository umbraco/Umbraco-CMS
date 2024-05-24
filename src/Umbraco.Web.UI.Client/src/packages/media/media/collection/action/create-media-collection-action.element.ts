import { UMB_MEDIA_COLLECTION_CONTEXT } from '../media-collection.context-token.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../../workspace/index.js';
import { UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../../entity.js';
import { html, customElement, property, state, map } from '@umbraco-cms/backoffice/external/lit';
import { UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import type { ManifestCollectionAction } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbAllowedMediaTypeModel } from '@umbraco-cms/backoffice/media-type';

@customElement('umb-create-media-collection-action')
export class UmbCreateMediaCollectionActionElement extends UmbLitElement {
	@state()
	private _allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];

	@state()
	private _createMediaPath = '';

	@state()
	private _currentView?: string;

	@state()
	private _mediaUnique?: string;

	@state()
	private _mediaTypeUnique?: string;

	@state()
	private _popoverOpen = false;

	@state()
	private _rootPathName?: string;

	@property({ attribute: false })
	manifest?: ManifestCollectionAction;

	#mediaTypeStructureRepository = new UmbMediaTypeStructureRepository(this);

	constructor() {
		super();

		new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath('media')
			.onSetup(() => {
				return { data: { entityType: 'media', preset: {} } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._createMediaPath = routeBuilder({});
			});

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.observe(workspaceContext.unique, (unique) => {
				this._mediaUnique = unique;
			});
			this.observe(workspaceContext.contentTypeUnique, (mediaTypeUnique) => {
				this._mediaTypeUnique = mediaTypeUnique;
			});
		});

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (collectionContext) => {
			this.observe(collectionContext.view.currentView, (currentView) => {
				this._currentView = currentView?.meta.pathName;
			});
			this.observe(collectionContext.view.rootPathName, (rootPathName) => {
				this._rootPathName = rootPathName;
			});
		});
	}

	async firstUpdated() {
		this.#retrieveAllowedMediaTypesOf(this._mediaTypeUnique ?? '');
	}

	async #retrieveAllowedMediaTypesOf(unique: string | null) {
		const { data } = await this.#mediaTypeStructureRepository.requestAllowedChildrenOf(unique);
		if (data && data.items) {
			this._allowedMediaTypes = data.items;
		}
	}

	// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
	// @ts-ignore
	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	#getCreateUrl(item: UmbAllowedMediaTypeModel) {
		return (
			this._createMediaPath.replace(`${this._rootPathName}`, `${this._rootPathName}/${this._currentView}`) +
			UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN.generateLocal({
				parentEntityType: this._mediaUnique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
				parentUnique: this._mediaUnique ?? 'null',
				documentTypeUnique: item.unique,
			})
		);
	}

	render() {
		return this._allowedMediaTypes.length !== 1 ? this.#renderDropdown() : this.#renderCreateButton();
	}

	#renderCreateButton() {
		if (this._allowedMediaTypes.length !== 1) return;

		const item = this._allowedMediaTypes[0];
		const label = (this.manifest?.meta.label ?? this.localize.term('general_create')) + ' ' + item.name;

		return html`<uui-button
			color="default"
			href=${this.#getCreateUrl(item)}
			label=${label}
			look="outline"></uui-button>`;
	}

	#renderDropdown() {
		if (!this._allowedMediaTypes.length) return;

		const label = this.manifest?.meta.label
			? this.localize.string(this.manifest.meta.label)
			: this.manifest?.name ?? '';

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
