import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, nothing, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_DRAWER_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/drawer';
import type { ManifestDrawerApp } from '@umbraco-cms/backoffice/drawer';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

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

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (context) => {
			if (!context) return;
			this.observe(context.modals, () => this.#repromoteIfOpen());
		});
	}

	async #onCurrentChanged(alias: string | undefined) {
		this._drawerEl?.classList.remove('is-closing');
		// Closed
		if (!alias) {
			if (!this._drawerEl) return;
			this._drawerEl.classList.add('is-closing');
			this._drawerEl.addEventListener(
				'transitionend',
				() => {
					if (!this._drawerEl?.classList.contains('is-closing')) return;
					this._drawerEl.classList.remove('is-closing');
					this._drawerEl.hidePopover?.();
				},
				{ once: true },
			);
			return;
		}

		// Same alias with existing element — just re-show (preserves state)
		if (alias === this.#currentAlias && this._activeElement) {
			if (!this._drawerEl?.matches(':popover-open')) {
				this._drawerEl?.showPopover?.();
			}
			return;
		}

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
		await this.updateComplete;
		if (!this._drawerEl?.matches(':popover-open')) {
			this._drawerEl?.showPopover?.();
		}
	}

	#repromoteIfOpen() {
		// Only re-promote if a drawer is actually open
		if (!this.#currentAlias) return;
		this._drawerEl?.hidePopover?.();
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
				height: 100vh;
				transform: translateX(0);
				transition: transform 300ms ease-in-out;
			}

			@keyframes umb-drawer-slide-in {
				from {
					transform: translateX(100%);
				}
				to {
					transform: translateX(0);
				}
			}

			#drawer:popover-open {
				animation: umb-drawer-slide-in 300ms ease-in-out;
			}

			#drawer.is-closing {
				transform: translateX(100%);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-backoffice-drawer': UmbBackofficeDrawerElement;
	}
}
