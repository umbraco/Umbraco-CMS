import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import type { UmbExtensionElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestHeaderApp } from '@umbraco-cms/backoffice/extension-registry';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-backoffice-header-apps')
export class UmbBackofficeHeaderAppsElement extends LitElement {
	override render() {
		return html`
			<umb-extension-slot
				id="apps"
				type="headerApp"
				data-mark="header-apps"
				.renderMethod=${this.#extensionSlotRenderMethod}></umb-extension-slot>
		`;
	}

	#extensionSlotRenderMethod = (ext: UmbExtensionElementInitializer<ManifestHeaderApp>) => {
		if (ext.component) {
			ext.component.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'header-app:' + ext.manifest?.alias);
		}
		return ext.component;
	};

	static override styles: CSSResultGroup = [
		css`
			#apps {
				display: flex;
				align-items: center;
				gap: var(--uui-size-space-2);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-header-apps': UmbBackofficeHeaderAppsElement;
	}
}
