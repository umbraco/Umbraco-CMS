import type { UmbPropertyEditorDataSourceItemModel } from '../types.js';
import { css, customElement, html, ifDefined, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-property-editor-data-source-item-ref')
export class UmbPropertyEditorDataSourceItemRefElement extends UmbLitElement {
	#item?: UmbPropertyEditorDataSourceItemModel | undefined;

	@property({ type: Object })
	public get item(): UmbPropertyEditorDataSourceItemModel | undefined {
		return this.#item;
	}
	public set item(value: UmbPropertyEditorDataSourceItemModel | undefined) {
		this.#item = value;
	}

	@property({ type: Boolean })
	readonly = false;

	@property({ type: Boolean })
	standalone = false;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node
				name=${this.item.name}
				detail=${ifDefined(this.item.description)}
				?readonly=${this.readonly}
				?standalone=${this.standalone}>
				<slot name="actions" slot="actions"></slot>
				<umb-icon slot="icon" name=${this.item.icon ?? 'icon-database'}></umb-icon>
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			umb-user-avatar {
				font-size: var(--uui-size-4);
			}
		`,
	];
}

export { UmbPropertyEditorDataSourceItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-data-source-item-ref': UmbPropertyEditorDataSourceItemRefElement;
	}
}
