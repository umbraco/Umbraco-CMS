import type { ManifestDashboardApp } from '../dashboard-app.extension.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbLitElement {
	#defaultSize = 'small';

	#sizeMap = new Map([
		['small', 'small'],
		['medium', 'medium'],
		['large', 'large'],
	]);

	override render() {
		return html`
			<section id="content">
				<umb-extension-slot
					type="dashboardApp"
					.renderMethod=${this.#extensionSlotRenderMethod}
					class="grid-container"></umb-extension-slot>
			</section>
		`;
	}

	#extensionSlotRenderMethod = (ext: UmbExtensionElementInitializer<ManifestDashboardApp>) => {
		if (ext.component && ext.manifest) {
			const size = this.#sizeMap.get(ext.manifest.meta?.size) ?? this.#defaultSize;
			const headline = ext.manifest?.meta?.headline ? this.localize.string(ext.manifest?.meta?.headline) : undefined;
			return html`<uui-box part="umb-dashboard-app-${size}" headline=${ifDefined(headline)}>${ext.component}</uui-box>`;
		}

		return nothing;
	};

	static override styles = [
		UmbTextStyles,
		css`
			#content {
				padding: var(--uui-size-layout-1);
			}

			.grid-container {
				display: grid;
				grid-template-columns: repeat(4, 1fr);
				grid-template-rows: repeat(100, 225px);
				margin: calc(var(--uui-size-space-3) * -1);
			}

			umb-extension-slot::part(umb-dashboard-app-small) {
				grid-column: span 1;
				grid-row: span 1;
				margin: var(--uui-size-space-3);
			}

			umb-extension-slot::part(umb-dashboard-app-medium) {
				grid-column: span 2;
				grid-row: span 2;
				margin: var(--uui-size-space-3);
			}

			umb-extension-slot::part(umb-dashboard-app-large) {
				grid-column: span 2;
				grid-row: span 3;
				margin: var(--uui-size-space-3);
			}
		`,
	];
}

export { UmbDashboardElement as element };

declare global {
	interface HTMLElementTagNameMap {
		['umb-dashboard']: UmbDashboardElement;
	}
}
