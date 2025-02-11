import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import type { UmbDocumentItemModel } from './types.js';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import {
	classMap,
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	property,
	state,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-document-item-ref')
export class UmbDocumentItemRefElement extends UmbLitElement {
	#item?: UmbDocumentItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbDocumentItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbDocumentItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (!this.#item) {
			this.#modalRoute?.destroy();
			return;
		}

		if (oldValue?.unique !== this.#item.unique) {
			this.#modalRoute = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
				.addAdditionalPath(UMB_DOCUMENT_ENTITY_TYPE + '/' + this.#item.unique)
				.onSetup(() => {
					return { data: { entityType: UMB_DOCUMENT_ENTITY_TYPE, preset: {} } };
				})
				.observeRouteBuilder((routeBuilder) => {
					this._editPath = routeBuilder({});
				});
		}
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	@state()
	_editPath = '';

	@state()
	_defaultCulture?: string;

	@state()
	_appCulture?: string;

	@state()
	_propertyDataSetCulture?: UmbVariantId;

	#modalRoute?: any;

	constructor() {
		super();

		this.consumeContext(UMB_APP_LANGUAGE_CONTEXT, (context) => {
			this.observe(context.appLanguageCulture, (culture) => (this._appCulture = culture));
			this.observe(context.appDefaultLanguage, (value) => {
				this._defaultCulture = value?.unique;
			});
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (context) => {
			this._propertyDataSetCulture = context.getVariantId();
		});
	}

	#findVariant(culture: string | undefined) {
		return this.item?.variants.find((x) => x.culture === culture);
	}

	#getCurrentVariant() {
		if (this.#isInvariant()) {
			return this.item?.variants?.[0];
		}

		const culture = this._propertyDataSetCulture?.culture || this._appCulture;
		return this.#findVariant(culture);
	}

	#isInvariant() {
		const firstVariant = this.item?.variants?.[0];
		return firstVariant?.culture === null;
	}

	#getName() {
		const variant = this.#getCurrentVariant();
		const fallbackName = this.#findVariant(this._defaultCulture)?.name;

		return variant?.name ?? `(${fallbackName})`;
	}

	#isDraft() {
		const variant = this.#getCurrentVariant();
		return variant?.state === 'Draft';
	}

	#getHref(item: UmbDocumentItemModel) {
		return `${this._editPath}/edit/${item.unique}`;
	}

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				class=${classMap({ draft: this.#isDraft() })}
				name=${this.#getName()}
				href=${ifDefined(this.#getHref(this.item))}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				${this.#renderIcon(this.item)} ${this.#renderIsTrashed(this.item)}
			</uui-ref-node>
		`;
	}

	#renderIcon(item: UmbDocumentItemModel) {
		if (!item.documentType.icon) return;
		return html`<umb-icon slot="icon" name=${item.documentType.icon}></umb-icon>`;
	}

	#renderIsTrashed(item: UmbDocumentItemModel) {
		if (!item.isTrashed) return;
		return html`<uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag>`;
	}

	static override styles = [
		css`
			.draft {
				opacity: 0.6;
			}
		`,
	];
}

export { UmbDocumentItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-item-ref': UmbDocumentItemRefElement;
	}
}
