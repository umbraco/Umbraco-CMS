import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * A workspace info app that explains, in production runtime mode, that the editing surface is
 * read-only and that schema changes must be made in a local development environment and deployed.
 */
@customElement('umb-production-mode-info-app')
export class UmbProductionModeInfoAppElement extends UmbLitElement {
	override render() {
		return html`
			<uui-box>
				<div id="container">
					<umb-icon id="icon" name="icon-info"></umb-icon>
					<div>
						<strong><umb-localize key="general_productionMode">Production Mode</umb-localize></strong>
						<p><umb-localize key="general_runtimeModeProductionSchema"></umb-localize></p>
					</div>
				</div>
			</uui-box>
		`;
	}

	static override styles = [
		css`
			#container {
				display: flex;
				gap: var(--uui-size-space-4);
				align-items: flex-start;
			}

			#icon {
				flex: 0 0 auto;
				font-size: var(--uui-size-6);
				margin-top: 2px;
			}

			p {
				margin: var(--uui-size-space-2) 0 0;
			}
		`,
	];
}

export { UmbProductionModeInfoAppElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-production-mode-info-app': UmbProductionModeInfoAppElement;
	}
}
