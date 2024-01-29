import { css, html, customElement, property, repeat, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbMediaItemModel } from '@umbraco-cms/backoffice/media';
import { UmbMediaItemRepository } from '@umbraco-cms/backoffice/media';

@customElement('umb-user-media-start-node')
export class UmbUserMediaStartNodeElement extends UmbLitElement {
	@property({ type: Array, attribute: false })
	uniques: Array<string> = [];

	@state()
	_displayValue: Array<UmbMediaItemModel> = [];

	#itemRepository = new UmbMediaItemRepository(this);

	protected async firstUpdated(): Promise<void> {
		if (this.uniques.length === 0) return;
		const { data } = await this.#itemRepository.requestItems(this.uniques);
		this._displayValue = data || [];
	}

	render() {
		if (this.uniques.length < 1) {
			return html`
				<uui-ref-node name="Media Root">
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
					<uui-ref-node name=${ifDefined(item.variants[0]?.name)}>
						<uui-icon slot="icon" name="folder"></uui-icon>
					</uui-ref-node>
				`;
			},
		);
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbUserMediaStartNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-media-start-node': UmbUserMediaStartNodeElement;
	}
}
