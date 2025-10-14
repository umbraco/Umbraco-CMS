import type { UmbPoolingConfig, UmbPoolingInterval } from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDropdownElement } from '@umbraco-cms/backoffice/components';
import { consume } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-log-viewer-polling-button')
export class UmbLogViewerPollingButtonElement extends UmbLitElement {
	@query('#polling-rate-dropdown')
	private _dropdownElement?: UmbDropdownElement;

	@state()
	private _poolingConfig: UmbPoolingConfig = { enabled: false, interval: 0 };

	#pollingIntervals: UmbPoolingInterval[] = [2000, 5000, 10000, 20000, 30000];

	#logViewerContext?: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE;

	@consume({ context: UMB_APP_LOG_VIEWER_CONTEXT })
	private set _logViewerContext(value: typeof UMB_APP_LOG_VIEWER_CONTEXT.TYPE | undefined) {
		this.#logViewerContext = value;
		this.#observePoolingConfig();
	}
	private get _logViewerContext() {
		return this.#logViewerContext;
	}

	#observePoolingConfig() {
		this.observe(this._logViewerContext?.polling, (poolingConfig) => {
			this._poolingConfig = poolingConfig ? { ...poolingConfig } : { enabled: false, interval: 0 };
		});
	}

	#togglePolling() {
		this._logViewerContext?.togglePolling();
	}

	#setPolingInterval = (interval: UmbPoolingInterval) => {
		this._logViewerContext?.setPollingInterval(interval);

		this.#closePoolingPopover();
	};

	#closePoolingPopover() {
		if (this._dropdownElement) {
			this._dropdownElement.open = false;
		}

		this.#togglePolling();
	}

	override render() {
		return html`
			<uui-button-group>
				<uui-button label="Start pooling" @click=${this.#togglePolling}
					>${this._poolingConfig.enabled
						? html`<uui-icon name="icon-axis-rotation" id="polling-enabled-icon"></uui-icon>Polling
								${this._poolingConfig.interval / 1000} seconds`
						: 'Polling'}</uui-button
				>

				<umb-dropdown id="polling-rate-dropdown" compact label="Choose pooling time">
					${this.#pollingIntervals.map(
						(interval: UmbPoolingInterval) =>
							html`<uui-menu-item
								label="Every ${interval / 1000} seconds"
								@click-label=${() => this.#setPolingInterval(interval)}></uui-menu-item>`,
					)}
				</umb-dropdown>
			</uui-button-group>
		`;
	}

	static override styles = [
		css`
			#polling-enabled-icon {
				margin-right: var(--uui-size-space-3);
				margin-bottom: 1px;
				-webkit-animation: rotate-center 0.8s ease-in-out infinite both;
				animation: rotate-center 0.8s ease-in-out infinite both;
			}

			@-webkit-keyframes rotate-center {
				0% {
					-webkit-transform: rotate(0);
					transform: rotate(0);
				}
				100% {
					-webkit-transform: rotate(360deg);
					transform: rotate(360deg);
				}
			}
			@keyframes rotate-center {
				0% {
					-webkit-transform: rotate(0);
					transform: rotate(0);
				}
				100% {
					-webkit-transform: rotate(360deg);
					transform: rotate(360deg);
				}
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-polling-button': UmbLogViewerPollingButtonElement;
	}
}
