import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	override render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			<div id="slot"><slot></slot></div>
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-space-5);
				justify-content: space-between;
				width: 100%;
			}
			#slot {
				flex: 1;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
