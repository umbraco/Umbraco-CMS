import {
	DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	type UmbDocumentTypeItemModel,
} from '@umbraco-cms/backoffice/document-type';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-block-type-card')
export class UmbBlockTypeCardElement extends UmbLitElement {
	//
	#itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		(x) => x.unique,
	);

	@property({ type: String, attribute: false })
	href?: string;

	@property({ type: String, attribute: false })
	name?: string;

	@property({ type: String, attribute: false })
	iconColor?: string;

	@property({ type: String, attribute: false })
	backgroundColor?: string;

	// TODO: support custom icon/image file

	@property({ type: String, attribute: false })
	public get contentElementTypeKey(): string | undefined {
		return this._elementTypeKey;
	}
	public set contentElementTypeKey(value: string | undefined) {
		this._elementTypeKey = value;
		if (value) {
			this.#itemManager.setUniques([value]);
		} else {
			this.#itemManager.setUniques([]);
		}
	}
	private _elementTypeKey?: string | undefined;

	@state()
	_fallbackName?: string;

	@state()
	_fallbackIcon?: string | null;

	constructor() {
		super();

		this.observe(this.#itemManager.items, (items) => {
			const item = items[0];
			if (item) {
				console.log('got item', item);
				this._fallbackIcon = item.icon;
				this._fallbackName = item.name;
			}
		});
	}

	// TODO: Support image files instead of icons.
	render() {
		return html`
			<uui-card-block-type
				href=${ifDefined(this.href)}
				.name=${this.name ?? this._fallbackName ?? ''}
				.background=${this.backgroundColor}>
				<uui-icon name=${this._fallbackIcon ?? ''} style="color:${this.iconColor}"></uui-icon>
				<slot name="actions" slot="actions"> </slot>
			</uui-card-block-type>
		`;
	}
}

export default UmbBlockTypeCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-card': UmbBlockTypeCardElement;
	}
}
