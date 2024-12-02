import type { UmbInstallerContext } from '../installer.context.js';
import { UMB_INSTALLER_CONTEXT } from '../installer.context.js';
import type { CSSResultGroup } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';

import type {
	ConsentLevelPresentationModel,
	TelemetryResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { TelemetryLevelModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-installer-consent')
export class UmbInstallerConsentElement extends UmbLitElement {
	@state()
	private _telemetryLevels: ConsentLevelPresentationModel[] = [];

	@state()
	private _telemetryFormData?: TelemetryResponseModel['telemetryLevel'];

	private _installerContext?: UmbInstallerContext;

	constructor() {
		super();

		this.consumeContext(UMB_INSTALLER_CONTEXT, (installerContext) => {
			this._installerContext = installerContext;
			this._observeInstallerSettings();
			this._observeInstallerData();
		});
	}

	private _observeInstallerSettings() {
		if (!this._installerContext) return;

		this.observe(this._installerContext.settings, (settings) => {
			this._telemetryLevels = settings?.user?.consentLevels ?? [];
		});
	}

	private _observeInstallerData() {
		if (!this._installerContext) return;

		this.observe(this._installerContext.data, (data) => {
			this._telemetryFormData = data.telemetryLevel;
		});
	}

	private _handleChange(e: InputEvent) {
		const target = e.target as HTMLInputElement;

		const value: { [key: string]: string } = {};
		value[target.name] = this._telemetryLevels[parseInt(target.value) - 1].level ?? TelemetryLevelModel.DETAILED;
		this._installerContext?.appendData(value);
	}

	private _onNext() {
		this._installerContext?.nextStep();
	}

	private _onBack() {
		this._installerContext?.prevStep();
	}

	private get _selectedTelemetryIndex() {
		return this._telemetryLevels?.findIndex((x) => x.level === this._telemetryFormData) ?? 0;
	}

	private get _selectedTelemetry() {
		return this._telemetryLevels?.find((x) => x.level === this._telemetryFormData) ?? this._telemetryLevels[0];
	}

	private _renderSlider() {
		if (!this._telemetryLevels || this._telemetryLevels.length < 1) return;

		return html`
			<uui-slider
				@input=${this._handleChange}
				name="telemetryLevel"
				label="telemetry-level"
				value=${this._selectedTelemetryIndex + 1}
				hide-step-values
				min="1"
				max=${this._telemetryLevels.length}></uui-slider>
			<h2>${this._selectedTelemetry.level}</h2>
			<p>${unsafeHTML(this._selectedTelemetry.description)}</p>
		`;
	}

	override render() {
		return html`
			<div id="container" class="uui-text" data-test="installer-telemetry">
				<h1>Consent for telemetry data</h1>
				${this._renderSlider()}
				<div id="buttons">
					<uui-button label="Back" @click=${this._onBack} look="secondary"></uui-button>
					<uui-button id="button-install" @click=${this._onNext} label="Next" look="primary"></uui-button>
				</div>
			</div>
		`;
	}

	static override styles: CSSResultGroup = [
		css`
			:host,
			#container {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			uui-form {
				height: 100%;
			}

			form {
				height: 100%;
				display: flex;
				flex-direction: column;
			}

			h1 {
				text-align: center;
				margin-bottom: var(--uui-size-layout-3);
			}

			#buttons {
				display: flex;
				margin-top: auto;
			}

			#button-install {
				margin-left: auto;
				min-width: 120px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-installer-consent': UmbInstallerConsentElement;
	}
}
