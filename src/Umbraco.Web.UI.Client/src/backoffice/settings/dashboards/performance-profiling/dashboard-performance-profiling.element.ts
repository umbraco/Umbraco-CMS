import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { ProfilingResource } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dashboard-performance-profiling')
export class UmbDashboardPerformanceProfilingElement extends UmbLitElement {
	

	@state()
	private _profilingStatus?: boolean;

	@state()
	private _profilingPerformance = false;

	constructor() {
		super();
		this._getProfilingStatus();
		this._profilingPerformance = localStorage.getItem('profilingPerformance') === 'true';
	}

	private async _getProfilingStatus() {
		const { data } = await tryExecuteAndNotify(this, ProfilingResource.getProfilingStatus());

		if (data) {
			this._profilingStatus = data.enabled;
		}
	}

	private _changeProfilingPerformance() {
		this._profilingPerformance = !this._profilingPerformance;
		localStorage.setItem('profilingPerformance', this._profilingPerformance.toString());
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
						.checked="${this._profilingPerformance}"
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
	
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
			
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
}

export default UmbDashboardPerformanceProfilingElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-performance-profiling': UmbDashboardPerformanceProfilingElement;
	}
}
