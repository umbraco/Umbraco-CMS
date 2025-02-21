import type {
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue,
} from './entity-create-option-action-list-modal.token.js';
import { UmbRefItemElement } from '@umbraco-cms/backoffice/components';
import type { ManifestEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import {
	html,
	customElement,
	state,
	repeat,
	ifDefined,
	type PropertyValues,
	css,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

type ManifestType = ManifestEntityCreateOptionAction;

@customElement('umb-entity-create-option-action-list-modal')
export class UmbEntityCreateOptionActionListModalElement extends UmbModalBaseElement<
	UmbEntityCreateOptionActionListModalData,
	UmbEntityCreateOptionActionListModalValue
> {
	@state()
	private _apiControllers: Array<UmbExtensionApiInitializer<ManifestType>> = [];

	@state()
	_hrefList: Array<string | undefined> = [];

	protected override updated(_changedProperties: PropertyValues): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			this.#initApi();
		}
	}

	#initApi() {
		const data = this.data;
		if (!data) throw new Error('No data found');

		if (!data.entityType) throw new Error('No entityType found');
		if (data.unique === undefined) throw new Error('No unique found');

		new UmbExtensionsApiInitializer(
			this,
			umbExtensionsRegistry,
			'entityCreateOptionAction',
			(manifest: ManifestType) => {
				return [{ entityType: data.entityType, unique: data.unique, meta: manifest.meta }];
			},
			(manifest: ManifestType) => manifest.forEntityTypes.includes(data.entityType),
			async (controllers) => {
				this._apiControllers = controllers as unknown as Array<UmbExtensionApiInitializer<ManifestType>>;
				const hrefPromises = this._apiControllers.map((controller) => controller.api?.getHref());
				this._hrefList = await Promise.all(hrefPromises);
			},
		);
	}

	async #onOpen(event: Event, controller: UmbExtensionApiInitializer<ManifestType>) {
		event.stopPropagation();

		if (!controller.api) {
			throw new Error('No API found');
		}

		await controller.api.execute();

		this._submitModal();
	}

	async #onNavigate(event: Event, href: string | undefined) {
		const refItemElement = event.composedPath().find((x) => x instanceof UmbRefItemElement) as UmbRefItemElement;

		// ignore click events if they are not on a ref item
		if (!refItemElement) {
			return;
		}

		if (!href) {
			throw new Error('No href found');
		}

		this._submitModal();
	}

	#getTarget(href?: string) {
		if (href && href.startsWith('http')) {
			return '_blank';
		}

		return '_self';
	}

	override render() {
		return html`
			<uui-dialog-layout headline="${this.localize.term('general_create')}">
				${this._apiControllers.length === 0
					? html`<div>No create options available.</div>`
					: html`
							<uui-ref-list>
								${repeat(
									this._apiControllers,
									(controller) => controller.manifest?.alias,
									(controller, index) => this.#renderRefItem(controller, index),
								)}
							</uui-ref-list>
						`}
				<uui-button
					slot="actions"
					label=${this.localize.term('general_cancel')}
					@click=${this._rejectModal}></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderRefItem(controller: UmbExtensionApiInitializer<ManifestType>, index: number) {
		const manifest = controller.manifest;
		if (!manifest) throw new Error('No manifest found');

		const label = manifest.meta.label ? this.localize.string(manifest.meta.label) : manifest.name;
		const description = manifest.meta.description ? this.localize.string(manifest.meta.description) : undefined;
		const href = this._hrefList[index];

		return html`
			<umb-ref-item
				name=${label}
				detail=${ifDefined(description)}
				icon=${manifest.meta.icon}
				href=${ifDefined(href)}
				target=${this.#getTarget(href)}
				@open=${(event: Event) => this.#onOpen(event, controller)}
				@click=${(event: Event) => this.#onNavigate(event, href)}>
			</umb-ref-item>
		`;
	}

	static override styles = [
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}
		`,
	];
}

export { UmbEntityCreateOptionActionListModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-create-option-action-list-modal': UmbEntityCreateOptionActionListModalElement;
	}
}
