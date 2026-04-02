import { LitElement, css, customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-view-loader')
export class UmbViewLoaderElement extends LitElement {
	// Note just LitElement, not Umbraco Element.

	override render() {
		return html` <uui-loader></uui-loader>`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				width: 100%;
				justify-content: center;
				height: 100%;
				align-items: center;
				opacity: 0;
				animation: fadeIn 240ms 240ms forwards;
			}

			@keyframes fadeIn {
				100% {
					opacity: 100%;
				}
			}
		`,
	];
}
