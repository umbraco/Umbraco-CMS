import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
// Import directly from token file to avoid circular dependency with modal package
import { UMB_MODAL_CONTEXT } from '../../../modal/context/modal.context-token.js';
import type { UmbModalContext } from '../../../modal/context/modal.context.js';

/**
 * A component that displays a "Forbidden" message when a user tries to access a route they do not have permission for.
 * This is typically used in routing scenarios where access control is enforced.
 * It informs the user that they do not have the necessary permissions to view the requested resource.
 * @element umb-route-forbidden
 */
@customElement('umb-route-forbidden')
export class UmbRouteForbiddenElement extends UmbLitElement {
	#modalContext?: UmbModalContext;

	@state()
	private _isInModal = false;

	constructor() {
		super();
		this.consumeContext(UMB_MODAL_CONTEXT, (context) => {
			this.#modalContext = context;
			this._isInModal = !!context;
		});
	}

	#close() {
		this.#modalContext?.reject();
	}

	override render() {
		return html`
			<div class="uui-text">
				<h4><umb-localize key="routing_routeForbiddenTitle">Access denied</umb-localize></h4>
				<umb-localize key="routing_routeForbiddenDescription">
					You do not have permission to access this resource. Please contact your administrator for assistance.
				</umb-localize>
				${this._isInModal
					? html`<uui-button
							look="secondary"
							label=${this.localize.term('general_close')}
							@click=${this.#close}></uui-button>`
					: nothing}
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
				gap: var(--uui-size-space-4);
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
