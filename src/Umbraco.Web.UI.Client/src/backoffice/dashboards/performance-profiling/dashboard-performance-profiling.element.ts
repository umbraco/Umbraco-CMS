import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';

import { ApiError, ProblemDetails, ProfilingResource } from '@umbraco-cms/backend-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbNotificationDefaultData, UmbNotificationService } from '@umbraco-cms/services';

@customElement('umb-dashboard-performance-profiling')
export class UmbDashboardPerformanceProfilingElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-toggle {
				font-weight: bold;
			}

			h4 {
				margin-bottom: 0;
			}

			h4 + p {
				margin-top: 0;
			}
		`,
	];

	@state()
	private _profilingStatus?: boolean;

	@state()
	private _profilingPerfomance = false;

	private _notificationService?: UmbNotificationService;

	private async _getProfilingStatus() {
		try {
			const status = await ProfilingResource.getProfilingStatus();
			this._profilingStatus = status.enabled;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = { message: error.message ?? 'Something went wrong' };
				this._notificationService?.peek('danger', { data });
			}
		}
	}

	constructor() {
		super();
		this.consumeAllContexts(['umbNotificationService'], (instances) => {
			this._notificationService = instances['umbNotificationService'];
		});
	}

	connectedCallback(): void {
		super.connectedCallback();
		this._getProfilingStatus();
		this._profilingPerfomance = localStorage.getItem('profilingPerformance') === 'true';
	}

	private _changeProfilingPerformance() {
		this._profilingPerfomance = !this._profilingPerfomance;
		localStorage.setItem('profilingPerformance', this._profilingPerfomance.toString());
	}

	private renderProfilingStatus() {
		return this._profilingStatus
			? html`
					<p>
						Umbraco currently runs in debug mode. This means you can use the built-in performance profiler to assess the
						performance when rendering pages.
					</p>
					<p>
						If you want to activate the profiler for a specific page rendering, simply add
						<strong>umbDebug=true</strong> to the querystring when requesting the page.
					</p>

					<p>
						If you want the profiler to be activated by default for all page renderings, you can use the toggle below.
						It will set a cookie in your browser, which then activates the profiler automatically. In other words, the
						profiler will only be active by default in your browser - not everyone else's.
					</p>

					<uui-toggle
						label="Activate the profiler by default"
						label-position="left"
						.checked="${this._profilingPerfomance}"
						@change="${this._changeProfilingPerformance}"></uui-toggle>

					<h4>Friendly reminder</h4>
					<p>
						You should never let a production site run in debug mode. Debug mode is turned off by setting
						Umbraco:CMS:Hosting:Debug to false in appsettings.json, appsettings.{Environment}.json or via an environment
						variable.
					</p>
			  `
			: html`
					<p>
						Umbraco currently does not run in debug mode, so you can't use the built-in profiler. This is how it should
						be for a production site.
					</p>
					<p>
						Debug mode is turned on by setting <b>debug="true"</b> on the <b>&lt;compilation /&gt;</b> element in
						web.config.
					</p>
			  `;
	}

	render() {
		return html`
			<uui-box>
				<h1>Performance Profiling</h1>
				${typeof this._profilingStatus === 'undefined' ? html`<uui-loader></uui-loader>` : this.renderProfilingStatus()}
			</uui-box>
		`;
	}
}

export default UmbDashboardPerformanceProfilingElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-performance-profiling': UmbDashboardPerformanceProfilingElement;
	}
}
