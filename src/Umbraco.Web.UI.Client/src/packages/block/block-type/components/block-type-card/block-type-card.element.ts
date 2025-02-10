import {
	UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	type UmbDocumentTypeItemModel,
} from '@umbraco-cms/backoffice/document-type';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import { transformServerPathToClientPath } from '@umbraco-cms/backoffice/utils';
import { UUICardEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-block-type-card')
export class UmbBlockTypeCardElement extends UmbLitElement {
	//
	#init: Promise<void>;
	#serverUrl: string = '';

	readonly #itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	);

	@property({ type: String })
	href?: string;

	@property({ type: String, attribute: false })
	public set iconFile(value: string) {
		value = transformServerPathToClientPath(value);
		if (value) {
			this.#init.then(() => {
				const url = new URL(value, this.#serverUrl);
				this._iconFile = url.href;
			});
		} else {
			this._iconFile = undefined;
		}
	}
	public get iconFile(): string | undefined {
		return this._iconFile;
	}

	@state()
	private _iconFile?: string | undefined;

	@property({ type: String, attribute: false })
	iconColor?: string;

	@property({ type: String, attribute: false })
	backgroundColor?: string;

	// TODO: support custom icon/image file

	@property({ type: String, attribute: false })
	public set contentElementTypeKey(value: string | undefined) {
		this._elementTypeKey = value;
		if (value) {
			this.#itemManager.setUniques([value]);
		} else {
			this.#itemManager.setUniques([]);
		}
	}
	public get contentElementTypeKey(): string | undefined {
		return this._elementTypeKey;
	}
	private _elementTypeKey?: string;

	@state()
	_name = '';

	@state()
	_description?: string;

	@state()
	_fallbackIcon?: string | null;

	constructor() {
		super();

		this.#init = this.getContext(UMB_APP_CONTEXT).then((appContext) => {
			this.#serverUrl = appContext.getServerUrl();
		});

		this.observe(this.#itemManager.items, (items) => {
			const item = items[0];
			if (item) {
				this._fallbackIcon = item.icon;
				this._name = item.name ? this.localize.string(item.name) : this.localize.term('general_unknown');
				this._description = this.localize.string(item.description);
			}
		});
	}

	readonly #onOpen = () => {
		this.dispatchEvent(new UUICardEvent(UUICardEvent.OPEN));
	};

	// TODO: Support image files instead of icons.
	override render() {
		return html`
			<uui-card-block-type
				href=${ifDefined(this.href)}
				@open=${this.#onOpen}
				.name=${this._name}
				.description=${this._description}
				.background=${this.backgroundColor}>
				${this._iconFile
					? html`<img src=${this._iconFile} alt="" />`
					: html`<umb-icon name=${this._fallbackIcon ?? ''} color=${ifDefined(this.iconColor)}></umb-icon>`}
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
