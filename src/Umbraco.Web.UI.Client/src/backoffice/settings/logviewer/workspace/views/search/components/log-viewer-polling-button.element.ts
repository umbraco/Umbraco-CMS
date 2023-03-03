import { UUIPopoverElement, UUISymbolExpandElement } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, query, state } from 'lit/decorators.js';
import {
	PoolingCOnfig,
	PoolingInterval,
	UmbLogViewerWorkspaceContext,
	UMB_APP_LOG_VIEWER_CONTEXT_TOKEN,
} from '../../../logviewer.context';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-log-viewer-polling-button')
export class UmbLogViewerPollingButtonElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#polling-interval-menu {
				margin: 0;
				padding: 0;
				width: 20ch;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
				display: flex;
				flex-direction: column;
				transform: translateX(calc((100% - 33px) * -1));
			}

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

	@query('#polling-popover')
	private _pollingPopover!: UUIPopoverElement;

	@query('#polling-expand-symbol')
	private _polingExpandSymbol!: UUISymbolExpandElement;

	@state()
	private _poolingConfig: PoolingCOnfig = { enabled: false, interval: 0 };

	#pollingIntervals: PoolingInterval[] = [2000, 5000, 10000, 20000, 30000];

	#logViewerContext?: UmbLogViewerWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_APP_LOG_VIEWER_CONTEXT_TOKEN, (instance) => {
			this.#logViewerContext = instance;
			this.#observePoolingConfig();
			this.#logViewerContext.getLogs();
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

	#setPolingInterval(interval: PoolingInterval) {
		this.#logViewerContext?.setPollingInterval(interval);
		this.#closePoolingPopover();
	}

	#openPoolingPopover() {
		this._pollingPopover.open = true;
		this._polingExpandSymbol.open = true;
	}

	#closePoolingPopover() {
		this._pollingPopover.open = false;
		this._polingExpandSymbol.open = false;
	}

	render() {
		return html` <uui-button-group>
			<uui-button label="Start pooling" @click=${this.#togglePolling}
				>${this._poolingConfig.enabled
					? html`<uui-icon name="umb:axis-rotation" id="polling-enabled-icon"></uui-icon>Polling
							${this._poolingConfig.interval / 1000} seconds`
					: 'Pooling'}</uui-button
			>
			<uui-popover placement="bottom-end" id="polling-popover" @close=${() => (this._polingExpandSymbol.open = false)}>
				<uui-button slot="trigger" compact label="Choose pooling time" @click=${this.#openPoolingPopover}>
					<uui-symbol-expand id="polling-expand-symbol"></uui-symbol-expand>
				</uui-button>

				<ul id="polling-interval-menu" slot="popover">
					${this.#pollingIntervals.map(
						(interval: PoolingInterval) =>
							html`<uui-menu-item
								label="Every ${interval / 1000} seconds"
								@click-label=${() => this.#setPolingInterval(interval)}></uui-menu-item>`
					)}
				</ul>
			</uui-popover>
		</uui-button-group>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-polling-button': UmbLogViewerPollingButtonElement;
	}
}
