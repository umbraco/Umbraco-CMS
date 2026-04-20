import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import './tree-view-bundle.element.js';

@customElement('umb-tree-toolbar')
export class UmbTreeToolbarElement extends UmbLitElement {
	override render() {
		return html`
			<div id="toolbar">
				<umb-tree-view-bundle></umb-tree-view-bundle>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#toolbar {
				display: flex;
				justify-content: flex-end;
				align-items: center;
				width: 100%;
				padding: var(--uui-size-space-3) var(--uui-size-space-4);
				box-sizing: border-box;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-toolbar': UmbTreeToolbarElement;
	}
}
