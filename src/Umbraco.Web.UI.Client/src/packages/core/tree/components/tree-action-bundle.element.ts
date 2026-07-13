import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-tree-action-bundle')
export class UmbTreeActionBundleElement extends UmbLitElement {
	override render() {
		return html`<umb-extension-with-api-slot type="treeAction"></umb-extension-with-api-slot>`;
	}

	static override readonly styles = [
		css`
			:host {
				display: contents;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-action-bundle': UmbTreeActionBundleElement;
	}
}
