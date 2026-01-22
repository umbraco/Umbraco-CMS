import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, query, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { ProfilingService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { consumeContext } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

@customElement('umb-dashboard-performance-profiling')
export class UmbDashboardPerformanceProfilingElement extends UmbLitElement {
	@state()
	private _profilingStatus = true;

	// TODO: Get this from the management api configuration when available
	@state()
	private _isDebugMode = true;

	@state()
	private _isLoading = true;

	@query('#toggle')
	private _toggle!: HTMLInputElement;

	@consumeContext({ context: UMB_NOTIFICATION_CONTEXT })
	private _notificationContext: typeof UMB_NOTIFICATION_CONTEXT.TYPE | undefined;

	#setToggle(value: boolean) {
		this._toggle.checked = value;
		this._profilingStatus = value;
		this._isLoading = false;
	}

	override async firstUpdated() {
		const status = await this.#getProfilingStatus();
		this.#setToggle(status);
	}

	async #getProfilingStatus() {
		const { data } = await tryExecute(this, ProfilingService.getProfilingStatus());

		return data?.enabled ?? false;
	}

	async #disableProfilingStatus() {
		this._isLoading = true;
		const { error } = await tryExecute(this, ProfilingService.putProfilingStatus({ body: { enabled: false } }));

		if (error) {
			this.#setToggle(true);
			return;
		}

		// Test that it was actually disabled
		const status = await this.#getProfilingStatus();

		if (status) {
			this.#setToggle(true);
			this._notificationContext?.peek('warning', {
				data: {
					headline: this.localize.term('profiling_errorDisablingProfilerTitle'),
					message: this.localize.term('profiling_errorDisablingProfilerDescription'),
				},
			});
			return;
		}

		this.#setToggle(false);
	}

	async #enableProfilingStatus() {
		this._isLoading = true;
		const { error } = await tryExecute(this, ProfilingService.putProfilingStatus({ body: { enabled: true } }));

		if (error) {
			this.#setToggle(false);
			return;
		}

		// Test that it was actually enabled
		const status = await this.#getProfilingStatus();

		if (!status) {
			this.#setToggle(false);
			this._notificationContext?.peek('warning', {
				data: {
					headline: this.localize.term('profiling_errorEnablingProfilerTitle'),
					message: this.localize.term('profiling_errorEnablingProfilerDescription'),
				},
			});
			return;
		}

		this.#setToggle(true);
	}

	#renderProfilingStatus() {
		return this._isDebugMode
			? html`
					<umb-localize key="profiling_performanceProfilingDescription"></umb-localize>

					<uui-toggle
						id="toggle"
						label=${this.localize.term('profiling_activateByDefault')}
						label-position="left"
						?checked="${this._profilingStatus}"
						?disabled="${this._isLoading}"
						@change="${() =>
							this._profilingStatus ? this.#disableProfilingStatus() : this.#enableProfilingStatus()}"></uui-toggle>

					${when(this._isLoading, () => html`<uui-loader-circle></uui-loader-circle>`)}

					<h4>
						<umb-localize key="profiling_reminder"></umb-localize>
					</h4>

					<umb-localize key="profiling_reminderDescription"></umb-localize>
				`
			: html`<umb-localize key="profiling_profilerEnabledDescription"></umb-localize>`;
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('profiling_performanceProfiling')}>
				${typeof this._profilingStatus === 'undefined'
					? html`<uui-loader></uui-loader>`
					: this.#renderProfilingStatus()}
			</uui-box>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
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
