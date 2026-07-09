import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * A component that displays a "Forbidden" message when a user tries to access a route they do not have permission for.
 * This is typically used in routing scenarios where access control is enforced.
 * It informs the user that they do not have the necessary permissions to view the requested resource.
 * @element umb-route-forbidden
 */
@customElement('umb-route-forbidden')
export class UmbRouteForbiddenElement extends UmbLitElement {
	override render() {
		return html`
			<div class="uui-text">
				<h4><umb-localize key="routing_routeForbiddenTitle">Access denied</umb-localize></h4>
				<umb-localize key="routing_routeForbiddenDescription">
					You do not have permission to access this resource. Please contact your administrator for assistance.
				</umb-localize>
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
				animation: fadeIn 2s 0s forwards;
			}

			@keyframes fadeIn {
				100% {
					opacity: 100%;
				}
			}
		`,
	];
}

export default UmbRouteForbiddenElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-route-forbidden': UmbRouteForbiddenElement;
	}
}
