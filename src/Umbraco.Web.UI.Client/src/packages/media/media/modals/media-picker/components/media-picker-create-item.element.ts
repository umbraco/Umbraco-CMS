import { UmbMediaDetailRepository } from '../../../repository/detail/media-detail.repository.js';
import { css, html, customElement, state, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UmbAllowedMediaTypeModel, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';

@customElement('umb-media-picker-create-item')
export class UmbMediaPickerCreateItemElement extends UmbLitElement {
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this); // used to get allowed media items
	#mediaDetailRepository = new UmbMediaDetailRepository(this); // used to get media type of node

	private _node: string | null = null;

	@property()
	public set node(value: string | null) {
		this._node = value;
		this.#getAllowedMediaTypes();
	}

	public get node() {
		return this._node;
	}

	@state()
	private _popoverOpen = false;

	@state()
	private _allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];

	async #getNodeMediaType() {
		if (!this._node) return null;

		const { data } = await this.#mediaDetailRepository.requestByUnique(this.node!);
		return data?.mediaType.unique ?? null;
	}

	async #getAllowedMediaTypes() {
		const mediaType = await this.#getNodeMediaType();

		const { data: allowedMediaTypes } = await this.#mediaTypeStructure.requestAllowedChildrenOf(mediaType, this._node);
		this._allowedMediaTypes = allowedMediaTypes?.items ?? [];
	}

	#onPopoverToggle(event: ToggleEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	override render() {
		return html`
			<uui-button
				popovertarget="collection-action-menu-popover"
				label=${this.localize.term('actions_create')}
				color="default"
				look="outline">
				${this.localize.term('actions_create')}
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${!this._allowedMediaTypes.length
							? html`<div id="not-allowed">${this.localize.term('mediaPicker_notAllowed')}</div>`
							: repeat(
									this._allowedMediaTypes,
									(item) => item.unique,
									(item) =>
										html`<uui-menu-item label=${item.name}>
											<umb-icon slot="icon" name=${item.icon ?? 'icon-circle-dotted'}></umb-icon>
										</uui-menu-item>`,
								)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static override styles = [
		css`
			#not-allowed {
				padding: var(--uui-size-space-3);
			}
		`,
	];
}

export default UmbMediaPickerCreateItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-picker-create-item': UmbMediaPickerCreateItemElement;
	}
}
