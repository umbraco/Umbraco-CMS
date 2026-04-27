import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, nothing, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DRAWER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/drawer';
import type { ManifestDrawerApp } from '@umbraco-cms/backoffice/drawer';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-backoffice-drawer')
export class UmbBackofficeDrawerElement extends UmbLitElement {
	@query('#drawer')
	private _drawerEl?: HTMLElement;

	@state()
	private _activeElement?: HTMLElement;

	#currentAlias?: string;

	constructor() {
		super();

		this.consumeContext(UMB_DRAWER_MANAGER_CONTEXT, (context) => {
			if (!context) return;
			this.observe(context.current, (alias) => this.#onCurrentChanged(alias));
		});
	}

	async #onCurrentChanged(alias: string | undefined) {
		// Closed
		if (!alias) {
			this.#currentAlias = undefined;
			this._activeElement = undefined;
			this._drawerEl?.hidePopover?.();
			return;
		}

		// Same drawer already shown — nothing to do
		if (alias === this.#currentAlias) return;

		this.#currentAlias = alias;

		const manifest = umbExtensionsRegistry.getByAlias<ManifestDrawerApp>(alias);
		if (!manifest) {
			this._activeElement = undefined;
			return;
		}

		const element = await createExtensionElement(manifest);

		// User may have opened a different drawer (or closed) while we awaited the import
		if (this.#currentAlias !== alias) return;

		this._activeElement = element;
		this._drawerEl?.showPopover?.();
	}

	override render() {
		return html`<div id="drawer" popover="manual">${this._activeElement ?? nothing}</div>`;
	}

	static override styles: CSSResultGroup = [
		UmbTextStyles,
		css`
			:host {
				display: contents;
			}

			#drawer {
				margin: 0;
				padding: 0;
				border: 0;
				background: var(--uui-color-surface);
				color: inherit;
				outline: 0;
				/* Placeholder positioning — full layout/animation comes in a later step */
				position: fixed;
				inset: 0 0 0 auto;
				width: min(480px, 100vw);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-drawer': UmbBackofficeDrawerElement;
	}
}
