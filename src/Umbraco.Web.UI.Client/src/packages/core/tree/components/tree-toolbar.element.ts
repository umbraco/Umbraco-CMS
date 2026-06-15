import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './tree-action-bundle.element.js';
import './tree-view-bundle.element.js';

@customElement('umb-tree-toolbar')
export class UmbTreeToolbarElement extends UmbLitElement {
	override render() {
		return html`
			<div id="toolbar">
				<umb-tree-action-bundle></umb-tree-action-bundle>
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
				justify-content: space-between;
				align-items: center;
				width: 100%;
				padding: 0 0 var(--uui-size-layout-1) 0;
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
