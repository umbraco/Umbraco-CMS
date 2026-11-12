import { UmbRemoveEvent } from '@umbraco-cms/backoffice/event';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Layout for a filter active value: the value's label and a remove button labelled with it.
 * @element umb-filter-active-value-layout
 * @slot - Replaces the rendered label, for example to show a colour swatch next to the name. Set `label` anyway: the
 * remove button's accessible label is built from it.
 * @fires remove - When the remove button is clicked.
 */
@customElement('umb-filter-active-value-layout')
export class UmbFilterActiveValueLayoutElement extends UmbLitElement {
	/**
	 * The value as text, for example the name of the resolved item. Rendered unless content is slotted, and always used
	 * for the accessible label of the remove button.
	 */
	@property({ type: String })
	label?: string;

	#onRemove = () => {
		this.dispatchEvent(new UmbRemoveEvent());
	};

	override render() {
		return html`
			<uui-tag look="secondary">
				<div id="content">
					<slot><span id="label">${this.label}</span></slot>
					<uui-button
						label=${this.label
							? this.localize.term('filter_removeValueLabel', this.label)
							: this.localize.term('general_remove')}
						compact
						@click=${this.#onRemove}>
						<uui-icon name="icon-wrong"></uui-icon>
					</uui-button>
				</div>
			</uui-tag>
		`;
	}

	static override styles = [
		css`
			:host {
				display: inline-flex;
			}

			uui-tag {
				padding-right: 0;
			}

			#content {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-1);
			}

			uui-button {
				--uui-button-height: auto;
				--uui-button-padding-left-factor: 0.5;
				--uui-button-padding-right-factor: 0.5;
				--uui-button-padding-top-factor: 0;
				--uui-button-padding-bottom-factor: 0;
				font-size: inherit;
			}

			uui-icon {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-filter-active-value-layout': UmbFilterActiveValueLayoutElement;
	}
}
