import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ProfilingResource } from '@umbraco-cms/backoffice/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

@customElement('umb-dashboard-performance-profiling')
export class UmbDashboardPerformanceProfilingElement extends UmbLitElement {
	@state()
	private _profilingStatus?: boolean;

	// TODO: Get this from the management api configuration when available
	@state()
	private _isDebugMode = true;

	firstUpdated() {
		this._getProfilingStatus();
	}

	private async _getProfilingStatus() {
		const { data } = await tryExecuteAndNotify(this, ProfilingResource.getProfilingStatus());

		if (data) {
			this._profilingStatus = data.enabled;
		}
	}

	private async _changeProfilingStatus() {
		const { error } = await tryExecuteAndNotify(
			this,
			ProfilingResource.putProfilingStatus({ requestBody: { enabled: !this._profilingStatus } })
		);

		if (!error) {
			this._profilingStatus = !this._profilingStatus;
		}
	}

	private renderProfilingStatus() {
		return this._isDebugMode
			? html`
					<p>
						Umbraco is running in debug mode. This means you can use the built-in performance profiler to assess
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
						.checked="${this._profilingStatus}"
						@change="${this._changeProfilingStatus}"></uui-toggle>

					<h4>Friendly reminder</h4>
					<p>
						You should never let a production site run in debug mode. Debug mode is turned off by setting
						<strong>Umbraco:CMS:Hosting:Debug</strong> to <strong>false</strong> in appsettings.json,
						appsettings.{Environment}.json or via an environment variable.
					</p>
			  `
			: html`
					<p>
						Umbraco is not running in debug mode, so you can't use the built-in profiler. This is how it should be for a
						production site.
					</p>
					<p>
						Debug mode is turned on by setting <strong>Umbraco:CMS:Hosting:Debug</strong> to <strong>true</strong> in
						appsettings.json, appsettings.{Environment}.json or via an environment variable.
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
