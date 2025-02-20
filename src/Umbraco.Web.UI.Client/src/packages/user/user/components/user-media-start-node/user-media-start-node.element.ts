import { html, customElement, property, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbMediaItemModel } from '@umbraco-cms/backoffice/media';
import { UmbMediaItemRepository } from '@umbraco-cms/backoffice/media';

@customElement('umb-user-media-start-node')
export class UmbUserMediaStartNodeElement extends UmbLitElement {
	#uniques: Array<string> = [];
	@property({ type: Array, attribute: false })
	public get uniques(): Array<string> {
		return this.#uniques;
	}
	public set uniques(value: Array<string>) {
		this.#uniques = value;

		if (this.#uniques.length > 0) {
			this.#observeItems();
		}
	}

	@property({ type: Boolean })
	readonly = false;

	@state()
	_displayValue: Array<UmbMediaItemModel> = [];

	#itemRepository = new UmbMediaItemRepository(this);

	async #observeItems() {
		const { asObservable } = await this.#itemRepository.requestItems(this.#uniques);

		this.observe(asObservable(), (data) => {
			this._displayValue = data || [];
		});
	}

	override render() {
		if (this.uniques.length < 1) {
			return html`
				<uui-ref-node
					name="Media Root"
					?disabled=${this.readonly}
					style="--uui-color-disabled-contrast: var(--uui-color-text)">
					<uui-icon slot="icon" name="folder"></uui-icon>
				</uui-ref-node>
			`;
		}

		return repeat(
			this._displayValue,
			(item) => item.unique,
			(item) => {
				return html`
					<!-- TODO: get correct variant name -->
					<uui-ref-node
						name=${ifDefined(item.variants[0]?.name)}
						?disabled=${this.readonly}
						style="--uui-color-disabled-contrast: var(--uui-color-text)">
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}
}

export default UmbUserMediaStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-media-start-node': UmbUserMediaStartNodeElement;
	}
}
