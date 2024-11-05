import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

// TODO: maybe move this to UI Library.
// TODO: add overwrites for customization.

/**
 * Popover layout element.
 * @element umb-popover-layout
 */
@customElement('umb-popover-layout')
export class UmbPopoverLayoutElement extends UmbLitElement {
	override render() {
		return html`<slot></slot>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				background-color: var(--uui-color-surface);
				display: block;
				border: 1px solid var(--uui-color-border);
				border-radius: var(--uui-border-radius);
				box-shadow: var(--uui-shadow-depth-3);
				overflow: clip;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-popover-layout': UmbPopoverLayoutElement;
	}
}
