import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-collection-toolbar')
export class UmbCollectionToolbarElement extends UmbLitElement {
	render() {
		return html`
			<umb-collection-action-bundle></umb-collection-action-bundle>
			<umb-collection-view-bundle></umb-collection-view-bundle>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				gap: var(--uui-size-space-5);
				width: 100%;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-toolbar': UmbCollectionToolbarElement;
	}
}
