import { css, html, customElement, state, repeat, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { type UmbAllowedMediaTypeModel, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';

@customElement('umb-media-picker-create-item')
export class UmbMediaPickerCreateItemElement extends UmbLitElement {
	#mediaTypeStructure = new UmbMediaTypeStructureRepository(this); // used to get allowed media items

	private _mediaTypeUnique: string | null = null;
	@property()
	public set mediaTypeUnique(value: string | null) {
		this._mediaTypeUnique = value;
		this.#getAllowedMediaTypes();
	}

	public get mediaTypeUnique() {
		return this._mediaTypeUnique;
	}

	@state()
	private _popoverOpen = false;

	@state()
	private _allowedMediaTypes: Array<UmbAllowedMediaTypeModel> = [];

	async #getAllowedMediaTypes() {
		const { data: allowedMediaTypes } = await this.#mediaTypeStructure.requestAllowedChildrenOf(this._mediaTypeUnique);
		this._allowedMediaTypes = allowedMediaTypes?.items ?? [];
	}

	#onPopoverToggle(event: ToggleEvent) {
		this._popoverOpen = event.newState === 'open';
	}

	render() {
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
										html`<uui-menu-item
											label=${item.name}
											@click=${() =>
												alert(
													'TODO: Open workspace (create) from modal. You can drop the files into this modal for now.',
												)}>
											<umb-icon slot="icon" name=${item.icon ?? 'icon-circle-dotted'}></umb-icon>
										</uui-menu-item>`,
								)}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	static styles = [
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
