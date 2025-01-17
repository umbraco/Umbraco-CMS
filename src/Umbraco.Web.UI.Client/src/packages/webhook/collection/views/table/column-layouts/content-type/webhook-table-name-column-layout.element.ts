import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, nothing, customElement, property, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbMediaTypeItemRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbDocumentTypeItemRepository } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-webhook-table-content-type-column-layout')
export class UmbWebhookTableContentTypeColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value?: { contentTypeName: string; contentTypes: Array<string> };

	@state()
	private _contentTypes = '';

	#repository?: UmbDocumentTypeItemRepository | UmbMediaTypeItemRepository;

	override connectedCallback(): void {
		super.connectedCallback();

		this.#getContentTypes();
	}

	async #getContentTypes() {
		switch (this.value?.contentTypeName) {
			case 'Content':
				this.#repository = new UmbDocumentTypeItemRepository(this);
				break;
			case 'Media':
				this.#repository = new UmbMediaTypeItemRepository(this);
				break;
		}

		if (this.value?.contentTypeName && this.#repository) {
			const { data } = await this.#repository.requestItems(this.value.contentTypes);
			this._contentTypes = data?.map((item) => this.localize.string(item.name)).join(', ') ?? '';
		}
	}

	override render() {
		if (!this.value) return nothing;

		return html`${this._contentTypes}`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				white-space: nowrap;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-content-type-column-layout': UmbWebhookTableContentTypeColumnLayoutElement;
	}
}
