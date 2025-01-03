import type {
	PoolingCOnfig,
	PoolingInterval,
	UmbLogViewerWorkspaceContext,
} from '../../../logviewer-workspace.context.js';
import { UMB_APP_LOG_VIEWER_CONTEXT } from '../../../logviewer-workspace.context-token.js';
import { css, html, customElement, query, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDropdownElement } from '@umbraco-cms/backoffice/components';

@customElement('umb-log-viewer-polling-button')
export class UmbLogViewerPollingButtonElement extends UmbLitElement {
	@query('#polling-rate-dropdown')
	private dropdownElement?: UmbDropdownElement;
	@state()
	private _poolingConfig: PoolingCOnfig = { enabled: false, interval: 0 };

	#pollingIntervals: PoolingInterval[] = [2000, 5000, 10000, 20000, 30000];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT, (instance) => {
			this.#logViewerContext = instance;
			this.#observePoolingConfig();
		});
	}

	#observePoolingConfig() {
		if (!this.#logViewerContext) return;

		this.observe(this.#logViewerContext.polling, (poolingConfig) => {
			this._poolingConfig = { ...poolingConfig };
		});
	}

	#togglePolling() {
		this.#logViewerContext?.togglePolling();
	}

	#setPolingInterval = (interval: PoolingInterval) => {
		this.#logViewerContext?.setPollingInterval(interval);

		this.#closePoolingPopover();
	};

	#closePoolingPopover() {
		if (this.dropdownElement) {
			this.dropdownElement.open = false;
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
						(interval: PoolingInterval) =>
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
