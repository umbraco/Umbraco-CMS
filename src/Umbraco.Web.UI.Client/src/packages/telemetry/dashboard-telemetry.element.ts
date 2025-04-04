import { css, html, customElement, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import type { TelemetryResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { TelemetryLevelModel, TelemetryService, ApiError } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-dashboard-telemetry')
export class UmbDashboardTelemetryElement extends UmbLitElement {
	@state()
	private _telemetryFormData = TelemetryLevelModel.BASIC;

	@state()
	private _telemetryLevels: TelemetryResponseModel[] = [];

	@state()
	private _errorMessage = '';

	@state()
	private _buttonState: UUIButtonState | undefined = undefined;

	constructor() {
		super();
		this._setup();
	}

	private async _setup() {
		const telemetryLevels = await tryExecute(this, TelemetryService.getTelemetry({ skip: 0, take: 3 }));
		this._telemetryLevels = telemetryLevels.data?.items ?? [];

		const telemetryLevel = await tryExecute(this, TelemetryService.getTelemetryLevel());
		this._telemetryFormData = telemetryLevel.data?.telemetryLevel ?? TelemetryLevelModel.BASIC;
	}

	private _handleSubmit = async (e: CustomEvent<SubmitEvent>) => {
		e.stopPropagation();

		this._buttonState = 'waiting';

		const { error } = await tryExecute(
			this,
			TelemetryService.postTelemetryLevel({
				requestBody: { telemetryLevel: this._telemetryFormData },
			}),
		);

		if (error) {
			this._buttonState = 'failed';
			this._errorMessage = error instanceof ApiError ? (error.body as any).detail : error.message;
			return;
		}

		this._buttonState = 'success';
	};

	private _handleChange(e: InputEvent) {
		const target = e.target as HTMLInputElement;
		this._telemetryFormData =
			this._telemetryLevels[parseInt(target.value) - 1].telemetryLevel ?? TelemetryLevelModel.BASIC;
	}

	private get _selectedTelemetryIndex() {
		return this._telemetryLevels.findIndex((x) => x.telemetryLevel === this._telemetryFormData) ?? 0;
	}

	private get _selectedTelemetry() {
		return this._telemetryLevels.find((x) => x.telemetryLevel === this._telemetryFormData) ?? this._telemetryLevels[1];
	}

	private get _selectedTelemetryDescription() {
		switch (this._selectedTelemetry.telemetryLevel) {
			case TelemetryLevelModel.MINIMAL:
				return this.localize.term('analytics_minimalLevelDescription');
			case TelemetryLevelModel.BASIC:
				return this.localize.term('analytics_basicLevelDescription');
			case TelemetryLevelModel.DETAILED:
				return this.localize.term('analytics_detailedLevelDescription');
			default:
				return 'Could not find description for this setting';
		}
	}

	private _renderSettingSlider() {
		if (!this._telemetryLevels || this._telemetryLevels.length < 1) return;

		return html`
			<uui-slider
				@input=${this._handleChange}
				name="telemetryLevel"
				label=${this.localize.term('analytics_consentForAnalytics')}
				value=${this._selectedTelemetryIndex + 1}
				min="1"
				max=${this._telemetryLevels.length}
				hide-step-values
				hide-value-label></uui-slider>
			<h3>${this._selectedTelemetry.telemetryLevel}</h3>
			<p>${unsafeHTML(this._selectedTelemetryDescription)}</p>
		`;
	}

	override render() {
		return html`
			<uui-box headline=${this.localize.term('analytics_consentForAnalytics')}>
				<div style="max-width:75ch">
					<umb-localize key="analytics_analyticsDescription"></umb-localize>
					${this._renderSettingSlider()}
					<uui-button
						look="primary"
						color="positive"
						label=${this.localize.term('buttons_save')}
						@click="${this._handleSubmit}"
						.state=${this._buttonState}></uui-button>
				</div>
				${this._errorMessage ? html`<p class="error">${this._errorMessage}</p>` : ''}
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
		`,
	];
}

export default UmbDashboardTelemetryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-telemetry': UmbDashboardTelemetryElement;
	}
}
