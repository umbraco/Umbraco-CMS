import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './tree-action-bundle.element.js';
import './tree-view-bundle.element.js';

@customElement('umb-tree-toolbar')
export class UmbTreeToolbarElement extends UmbLitElement {
	/**
	 * When true the tree actions are hidden.
	 * Defaults to true — tree actions are not shown unless explicitly opted in with hide-tree-actions="false".
	 */
	@property({ type: Boolean, attribute: 'hide-tree-actions' })
	hideTreeActions: boolean = true;

	override render() {
		return html`
			<div id="toolbar">
				${!this.hideTreeActions ? html`<umb-tree-action-bundle></umb-tree-action-bundle>` : nothing}
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
				align-items: center;
				width: 100%;
				padding: 0 0 var(--uui-size-layout-1) 0;
				box-sizing: border-box;
			}

			umb-tree-view-bundle {
				display: inline-block;
				margin-left: auto;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-toolbar': UmbTreeToolbarElement;
	}
}
