import { css, customElement, html, nothing, state, when } from '@umbraco-cms/backoffice/external/lit';
import {
	umbExtensionsRegistry,
	UmbBackofficeEntryPointExtensionInitializer,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPreviewContext } from '@umbraco-cms/backoffice/preview';
import { UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';

const CORE_PACKAGES = [import('../../packages/preview/umbraco-package.js')];

/**
 * @element umb-preview
 */
@customElement('umb-preview')
export class UmbPreviewElement extends UmbLitElement {
	#context = new UmbPreviewContext(this);

	@state()
	private _iframeReady?: boolean;

	@state()
	private _previewUrl?: string;

	constructor() {
		super();

		new UmbBackofficeEntryPointExtensionInitializer(this, umbExtensionsRegistry);

		this.observe(this.#context.iframeReady, (iframeReady) => (this._iframeReady = iframeReady));
		this.observe(this.#context.previewUrl, (previewUrl) => (this._previewUrl = previewUrl));
	}

	override async firstUpdated() {
		await this.#extensionsAfterAuth();

		// Extensions are loaded in parallel and don't need to block the preview frame
		CORE_PACKAGES.forEach(async (packageImport) => {
			const { extensions } = await packageImport;
			umbExtensionsRegistry.registerMany(extensions);
		});
	}

	async #extensionsAfterAuth() {
		const authContext = await this.getContext(UMB_AUTH_CONTEXT, { preventTimeout: true });
		if (!authContext) {
			throw new Error('UmbPreviewElement requires the UMB_AUTH_CONTEXT to be set.');
		}
		await this.observe(authContext.isAuthorized).asPromise();
		await new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerPrivateExtensions();
	}

	#onIFrameLoad(event: Event & { target: HTMLIFrameElement }) {
		this.#context.iframeLoaded(event.target);
	}

	override render() {
		if (!this._previewUrl) return nothing;
		return html`
			${when(!this._iframeReady, () => html`<div id="loading"><uui-loader-circle></uui-loader-circle></div>`)}
			<div id="wrapper" class="fullsize">
				<div id="container">
					<iframe
						src=${this._previewUrl}
						title="Page preview"
						@load=${this.#onIFrameLoad}
						sandbox="allow-scripts allow-same-origin"></iframe>
				</div>
			</div>
			<div id="menu">
				<h4>Preview Mode</h4>
				<div id="apps">
					<umb-extension-slot type="previewApp"></umb-extension-slot>
				</div>
			</div>
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				justify-content: center;
				align-items: center;

				position: absolute;
				inset: 0;

				padding-bottom: 40px;
			}

			#loading {
				display: flex;
				align-items: center;
				justify-content: center;

				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;

				font-size: 6rem;
				backdrop-filter: blur(var(--uui-size-1, 3px));

				z-index: 1;
			}

			#wrapper {
				transition: all 240ms cubic-bezier(0.165, 0.84, 0.44, 1);
				flex-shrink: 0;
				height: 100%;
				width: 100%;
			}

			#wrapper.fullsize {
				margin: 0 auto;
				overflow: hidden;
			}

			#wrapper:not(.fullsize) {
				margin: 10px auto;
				background-color: white;
				border-radius: 3px;
				overflow: hidden;
				opacity: 1;
				box-shadow: 0 5px 20px 0 rgba(0, 0, 0, 0.26);
			}

			#container {
				width: 100%;
				height: 100%;
				margin: 0 auto;
				overflow: hidden;
			}

			#menu {
				display: flex;
				justify-content: space-between;
				align-items: center;

				position: absolute;
				bottom: 0;
				left: 0;
				right: 0;

				z-index: 1;

				background-color: var(--uui-color-header-surface);
				height: 40px;

				animation: menu-bar-animation 1.2s;
				animation-timing-function: cubic-bezier(0.23, 1, 0.32, 1);
			}

			#menu > h4 {
				color: var(--uui-color-header-contrast-emphasis);
				margin: 0;
				padding: 0 15px;
			}

			#apps {
				display: inline-flex;
				align-items: stretch;
				height: 100%;
			}

			uui-icon.flip {
				rotate: 90deg;
			}

			iframe {
				border: 0;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				width: 100%;
				height: 100%;
				overflow: hidden;
				overflow-x: hidden;
				overflow-y: hidden;
			}

			@keyframes menu-bar-animation {
				0% {
					bottom: -50px;
				}
				40% {
					bottom: -50px;
				}
				80% {
					bottom: 0px;
				}
			}
		`,
	];
}

export default UmbPreviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-preview': UmbPreviewElement;
	}
}
