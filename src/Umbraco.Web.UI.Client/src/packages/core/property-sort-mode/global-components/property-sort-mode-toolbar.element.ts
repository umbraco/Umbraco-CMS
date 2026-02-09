import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '../property-context/property-sort-mode.context-token.js';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-property-sort-mode-toolbar')
export class UmbPropertySortModeToolbarElement extends UmbLitElement {
	#onSortModeExit = async () => {
		const context = await this.getContext(UMB_PROPERTY_SORT_MODE_CONTEXT);
		context?.setIsSortMode(false);
	};

	override render() {
		return html`
			<div id="sort-mode">
				<uui-button
					look="secondary"
					label=${this.localize.term('blockEditor_actionExitSortMode')}
					@click=${this.#onSortModeExit}></uui-button>
			</div>
		`;
	}

	static override readonly styles = [
		css`
			#sort-mode {
				background-color: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
				box-sizing: border-box;
				border-radius: var(--uui-border-radius);
				display: flex;
				gap: var(--uui-size-3);
				padding: var(--uui-size-space-4) var(--uui-size-space-6);
				width: 100%;
				align-items: center;
				justify-content: flex-end;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-sort-mode-toolbar': UmbPropertySortModeToolbarElement;
	}
}
