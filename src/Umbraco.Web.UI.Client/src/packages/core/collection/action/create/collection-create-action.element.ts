import { html, customElement, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

type ManifestType = ManifestEntityCreateOptionAction;

const elementName = 'umb-collection-create-action-button';
@customElement(elementName)
export class UmbCollectionCreateActionButtonElement extends UmbLitElement {
	@state()
	private _popoverOpen = false;

	@state()
	private _multipleOptions = false;

	@state()
	private _apiControllers: Array<UmbExtensionApiInitializer<ManifestType>> = [];

	@state()
	_hrefList: Array<string | undefined> = [];

	#createLabel = this.localize.term('general_create');
	#entityContext?: typeof UMB_ENTITY_CONTEXT.TYPE;

	#onPopoverToggle(event: PointerEvent) {
		// TODO: This ignorer is just neede for JSON SCHEMA TO WORK, As its not updated with latest TS jet.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._popoverOpen = event.newState === 'open';
	}

	async #onClick(event: Event, controller: UmbExtensionApiInitializer<ManifestType>, href?: string) {
		event.stopPropagation();

		// skip if href is defined
		if (href) {
			return;
		}

		if (!controller.api) throw new Error('No API found');
		await controller.api.execute();
	}

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			this.#entityContext = context;
			this.#initApi();
		});
	}

	#initApi() {
		if (!this.#entityContext) return;

		const entityType = this.#entityContext.getEntityType();
		if (!entityType) throw new Error('No entityType found');

		const unique = this.#entityContext.getUnique();
		if (unique === undefined) throw new Error('No unique found');

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestType) => {
				return [{ entityType, unique, meta: manifest.meta }];
			},
			(manifest: ManifestType) => manifest.forEntityTypes.includes(entityType),
			async (controllers) => {
				this._apiControllers = controllers as unknown as Array<UmbExtensionApiInitializer<ManifestType>>;
				this._multipleOptions = controllers.length > 1;
				const hrefPromises = this._apiControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
			},
		);
	}

	#getTarget(href?: string) {
		if (href && href.startsWith('http')) {
			return '_blank';
		}

		return '_self';
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
						${this._apiControllers.map((controller, index) => this.#renderMenuItem(controller, index))}
					</uui-scroll-container>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	#renderMenuItem(controller: UmbExtensionApiInitializer<ManifestType>, index: number) {
		const manifest = controller.manifest;
		if (!manifest) throw new Error('No manifest found');

		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
		const href = this._hrefList[index];

		return html`
			<uui-menu-item
				label=${label}
				@click=${(event: Event) => this.#onClick(event, controller, href)}
				href=${ifDefined(href)}
				target=${this.#getTarget(href)}>
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
