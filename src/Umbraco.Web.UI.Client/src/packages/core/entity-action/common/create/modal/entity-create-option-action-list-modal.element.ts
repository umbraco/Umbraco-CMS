import type {
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue,
} from './entity-create-option-action-list-modal.token.js';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

const elementName = 'umb-entity-create-option-action-list-modal';
@customElement(elementName)
export class UmbEntityCreateOptionActionListModalElement extends UmbModalBaseElement<
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue
> {
	@state()
	private _apiControllers: Array<any> = [];

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
			},
		);
	}

	async #onClick(event: Event, controller: any) {
		event.stopPropagation();
		await controller.api.execute();
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('user_createUser')}">
				<uui-box>
					<uui-ref-list>
						${repeat(
							this._apiControllers,
							(controller) => controller.manifest.alias,
							(controller) => this.#renderRefItem(controller),
						)}
					</uui-ref-list>
				</uui-box>
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
			</umb-body-layout>
		`;
	}

	#renderRefItem(controller: any) {
		const manifest = controller.manifest;
		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.meta.name;

		return html`
			<umb-ref-item
				name=${label}
				detail=${manifest.meta.description}
				icon=${manifest.meta.icon}
				@click=${(event: Event) => this.#onClick(event, controller)}></umb-ref-item>
		`;
	}
}

export { UmbEntityCreateOptionActionListModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityCreateOptionActionListModalElement;
	}
}
