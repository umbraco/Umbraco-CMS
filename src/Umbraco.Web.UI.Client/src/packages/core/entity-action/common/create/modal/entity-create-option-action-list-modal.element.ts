import type {
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue,
} from './entity-create-option-action-list-modal.token.js';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, state, repeat, ifDefined, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

type ManifestType = ManifestEntityCreateOptionAction;

const elementName = 'umb-entity-create-option-action-list-modal';
@customElement(elementName)
export class UmbEntityCreateOptionActionListModalElement extends UmbModalBaseElement<
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue
> {
	@state()
	private _apiControllers: Array<UmbExtensionApiInitializer<ManifestType>> = [];

	@state()
	_hrefMap = new Map<string, string>();

	@state()
	_hrefList: Array<any> = [];

	@property({ attribute: false })
	public override set data(value: UmbEntityCreateOptionActionListModalData | undefined) {
		super.data = value;

		if (value) {
			this.#initApi();
		}
	}

	#initApi() {
		if (this._data?.entityType === undefined) throw new Error('No entityType found');
		if (this._data?.unique === undefined) throw new Error('No unique found');

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestType) => {
				return [{ entityType: this._data!.entityType, unique: this._data!.unique, meta: manifest.meta }];
			},
			undefined,
			async (controllers) => {
				this._apiControllers = controllers as unknown as Array<UmbExtensionApiInitializer<ManifestType>>;
				const hrefPromises = this._apiControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
			},
		);
	}

	async #onClick(event: Event, controller: UmbExtensionApiInitializer<ManifestType>) {
		event.stopPropagation();

		if (!controller.manifest) throw new Error('No manifest found');
		const href = this._hrefMap.get(controller.manifest?.alias);

		// skip if href is defined
		if (href) {
			return;
		}

		if (!controller.api) throw new Error('No API found');
		await controller.api.execute();
	}

	#getTarget(href?: string) {
		if (href && href.startsWith('http')) {
			return '_blank';
		}

		return '_self';
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('user_createUser')}">
				<uui-box>
					<uui-ref-list>
						${repeat(
							this._apiControllers,
							(controller) => controller.manifest?.alias,
							(controller, index) => this.#renderRefItem(controller, index),
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

	#renderRefItem(controller: UmbExtensionApiInitializer<ManifestType>, index: number) {
		const manifest = controller.manifest;
		if (!manifest) throw new Error('No manifest found');

		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
		const href = this._hrefList[index];

		return html`
			<uui-ref-node
				name=${label}
				detail=${ifDefined(manifest.meta.description)}
				icon=${manifest.meta.icon}
				@click=${(event: Event) => this.#onClick(event, controller)}
				href=${ifDefined(href)}
				target=${this.#getTarget(href)}
				?selectable=${!href}
				?readonly=${!href}></uui-ref-node>
		`;
	}
}

export { UmbEntityCreateOptionActionListModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbEntityCreateOptionActionListModalElement;
	}
}
