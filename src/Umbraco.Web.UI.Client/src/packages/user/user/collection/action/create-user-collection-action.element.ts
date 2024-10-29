import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-create-user-collection-action-button';
@customElement(elementName)
export class UmbCollectionActionButtonElement extends UmbLitElement {
	@state()
	private _popoverOpen = false;

	@state()
	private _multipleOptions = false;

	#apiControllers: Array<any> = [];
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
				this.#apiControllers = controllers;
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
			@click=${(event: Event) => this.#onClick(event, this.#apiControllers[0])}></uui-button>`;
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
						${this.#apiControllers.map((controller) => this.#renderMenuItem(controller))}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderMenuItem(controller: any) {
		const label = controller.manifest.meta.label
			? this.localize.string(controller.manifest.meta.label)
			: controller.manifest.meta.name;

		return html`
			<uui-menu-item label=${label} @click=${(event: Event) => this.#onClick(event, controller)}>
				<umb-icon slot="icon" name="icon-user"></umb-icon>
			</uui-menu-item>
		`;
	}
}

export { UmbCollectionActionButtonElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionActionButtonElement;
	}
}
