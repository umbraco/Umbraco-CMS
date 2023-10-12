import { html, customElement, property, when, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-crops-table-input-column
 */
@customElement('umb-crops-table-input-column')
export class UmbCropsTableInputColumnElement extends UmbLitElement {
	@property({ type: Object })
	value?: any;

	#onChange(event: Event) {
		this.value = (event.target as HTMLInputElement).value;
	}

	render() {
		if (!this.value) return nothing;

		return html`
			<uui-input .value=${this.value.value} autocomplete="false">
				${when(this.value.append, () => html`<span id="append" slot="append">${this.value.append}</span>`)}
			</uui-input>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#append {
				padding-inline: var(--uui-size-space-4);
				background: var(--uui-color-disabled);
				border-left: 1px solid var(--uui-color-border);
				color: var(--uui-color-disabled-contrast);
				font-size: 0.8em;
				display: flex;
				align-items: center;
			}
		`,
	];
}

export default UmbCropsTableInputColumnElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-crops-table-input-column': UmbCropsTableInputColumnElement;
	}
}
