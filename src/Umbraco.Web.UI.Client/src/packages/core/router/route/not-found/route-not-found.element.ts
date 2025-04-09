import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * A fallback view to be used in Workspace Views, maybe this can be upgraded at a later point.
 */
@customElement('umb-route-not-found')
export class UmbRouteNotFoundElement extends UmbLitElement {
	override render() {
		return html`
			<div class="uui-text">
				<h4><umb-localize key="routing_routeNotFoundTitle"></umb-localize></h4>
				<umb-localize key="routing_routeNotFoundDescription"></umb-localize>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
				min-width: 0;
			}

			:host > div {
				display: flex;
				flex-direction: column;
				justify-content: center;
				align-items: center;
				height: 100%;
				opacity: 0;
				animation: fadeIn 6s 0.2s forwards;
			}

			@keyframes fadeIn {
				100% {
					opacity: 100%;
				}
			}
		`,
	];
}

export default UmbRouteNotFoundElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-route-not-found': UmbRouteNotFoundElement;
	}
}
