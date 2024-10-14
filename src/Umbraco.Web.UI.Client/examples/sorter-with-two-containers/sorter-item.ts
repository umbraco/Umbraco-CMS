import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, LitElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-sorter-item')
export class ExampleSorterItem extends UmbElementMixin(LitElement) {
	@property({ type: String, reflect: true })
	name: string = '';

	// TODO: Does it make any different to have this as a property?
	@property({ type: Boolean, reflect: true, attribute: 'drag-placeholder' })
	umbDragPlaceholder = false;

	override render() {
		return html`
			<div>
				${this.name}
				<img src="https://picsum.photos/seed/${this.name}/400/400" style="width:120px;" />
				<slot name="action"></slot>
			</div>
			<slot></slot>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				margin-bottom: 3px;
			}
			:host([drag-placeholder]) {
				opacity: 0.2;
			}

			div {
				display: flex;
				align-items: center;
				justify-content: space-between;
			}

			slot:not([name]) {
				// go on new line:
			}
		`,
	];
}

export default ExampleSorterItem;

declare global {
	interface HTMLElementTagNameMap {
		'example-sorter-item': ExampleSorterItem;
	}
}
