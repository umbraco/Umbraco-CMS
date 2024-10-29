import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-collection-create-action-button';
@customElement(elementName)
export class UmbCollectionCreateActionButtonElement extends UmbLitElement {
	@state()
	private _popoverOpen = false;

	@state()
	private _multipleOptions = false;

	@state()
	private _apiControllers: Array<any> = [];

	#createLabel = this.localize.term('general_create');

	#onPopoverToggle(event: PointerEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	async #onClick(event: Event, controller: any) {
		event.stopPropagation();
		await controller.api.execute();
	}

	constructor() {
		super();

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			[],
			undefined,
			(controllers) => {
				this._apiControllers = controllers;
				this._multipleOptions = controllers.length > 1;
			},
		);
	}

	override render() {
		return this._multipleOptions ? this.#renderMultiOptionAction() : this.#renderSingleOptionAction();
	}

	#renderSingleOptionAction() {
		return html` <uui-button
			label=${this.#createLabel}
			color="default"
			look="outline"
			@click=${(event: Event) => this.#onClick(event, this._apiControllers[0])}></uui-button>`;
	}

	#renderMultiOptionAction() {
		return html`
			<uui-button
				popovertarget="collection-action-menu-popover"
				label=${this.#createLabel}
				color="default"
				look="outline">
				${this.#createLabel}
				<uui-symbol-expand .open=${this._popoverOpen}></uui-symbol-expand>
			</uui-button>
			${this.#renderDropdown()}
		`;
	}

	#renderDropdown() {
		return html`
			<uui-popover-container
				id="collection-action-menu-popover"
				placement="bottom-start"
				@toggle=${this.#onPopoverToggle}>
				<umb-popover-layout>
					<uui-scroll-container>
						${this._apiControllers.map((controller) => this.#renderMenuItem(controller))}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderMenuItem(controller: any) {
		const manifest = controller.manifest;
		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.meta.name;

		return html`
			<uui-menu-item label=${label} @click=${(event: Event) => this.#onClick(event, controller)}>
				<umb-icon slot="icon" .name=${manifest.meta.icon}></umb-icon>
			</uui-menu-item>
		`;
	}
}

export { UmbCollectionCreateActionButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionCreateActionButtonElement;
	}
}
