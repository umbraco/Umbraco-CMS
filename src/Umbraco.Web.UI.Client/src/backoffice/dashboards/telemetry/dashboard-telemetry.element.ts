import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';

import { customElement, property, state } from 'lit/decorators.js';

import type { TelemetryModel } from '../../../core/models';
import { getConsentLevel, getConsentLevels } from '../../../core/api/fetcher';

export type SettingOption = 'Minimal' | 'Basic' | 'Detailed';

@customElement('umb-dashboard-telemetry')
export class UmbDashboardTelemetryElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			.italic {
				font-style: italic;
			}
		`,
	];

	@state()
	private _telemetryLevels: TelemetryModel[] = [
		{ level: 'Minimal', description: 'We will only send an anonymized site ID to let us know that the site exists.' },
		{ level: 'Basic', description: 'We will send an anonymized site ID, Umbraco version, and packages installed.' },
		{
			level: 'Detailed',
			description: `We will send:<ul>
			<li>Anonymized site ID, Umbraco version, and packages installed.</li>
			<li>Number of: Root nodes, Content nodes, Macros, Media, Document Types, Templates, Languages, Domains, User Group, Users, Members, and Property Editors in use.</li>
			<li>System information: Webserver, server OS, server framework, server OS language, and database provider.</li>
			<li>Configuration settings: Modelsbuilder mode, if custom Umbraco path exists, ASP environment, and if you are in debug mode.</li>
			</ul>
			
			<i>We might change what we send on the Detailed level in the future. If so, it will be listed above.
			By choosing "Detailed" you agree to current and future anonymized information being collected.</i>`,
		},
	];

	@state()
	private _telemetryFormData?: TelemetryModel['level'] = 'Basic';

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		getConsentLevel({}).then((data) => {
			console.log(data);
		});
		getConsentLevels({}).then((data) => {
			console.log(data);
		});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
	}

	private _handleChange(e: InputEvent) {
		const target = e.target as HTMLInputElement;
		this._telemetryFormData = this._telemetryLevels[parseInt(target.value) - 1].level;
	}

	private get _selectedTelemetryIndex() {
		return this._telemetryLevels?.findIndex((x) => x.level === this._telemetryFormData) ?? 0;
	}

	private get _selectedTelemetry() {
		return this._telemetryLevels?.find((x) => x.level === this._telemetryFormData) ?? this._telemetryLevels[0];
	}

	private _renderSettingSlider() {
		if (!this._telemetryLevels || this._telemetryLevels.length < 1) return;

		return html`
			<uui-slider
				@input=${this._handleChange}
				name="telemetryLevel"
				label="telemetry level"
				value=${this._selectedTelemetryIndex + 1}
				min="1"
				max=${this._telemetryLevels.length}
				hide-step-values></uui-slider>
			<h2>${this._selectedTelemetry.level}</h2>
			<p>${unsafeHTML(this._selectedTelemetry.description)}</p>
		`;
	}

	render() {
		return html`
			<uui-box>
				<h1>Consent for telemetry data</h1>
				<div style="max-width:580px">
					<p>
						In order to improve Umbraco and add new functionality based on as relevant information as possible, we would
						like to collect system- and usage information from your installation. Aggregate data will be shared on a
						regular basis as well as learnings from these metrics. Hopefully, you will help us collect some valuable
						data.
						<br /><br />
						We <strong>WILL NOT</strong> collect any personal data such as content, code, user information, and all data
						will be fully anonymized.
					</p>
					${this._renderSettingSlider()}
					<uui-button look="primary" color="positive" label="Save telemetry settings">Save</uui-button>
				</div>
			</uui-box>
		`;
	}
}

export default UmbDashboardTelemetryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-telemetry': UmbDashboardTelemetryElement;
	}
}
