import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { css, html, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';

import type { ModelsBuilderResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { ModelsBuilderService, ModelsModeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-models-builder-dashboard')
export class UmbModelsBuilderDashboardElement extends UmbLitElement {
	@state()
	private _modelsBuilder?: ModelsBuilderResponseModel;

	@state()
	private _buttonStateBuild: UUIButtonState = undefined;

	@state()
	private _buttonStateReload: UUIButtonState = undefined;

	constructor() {
		super();
		this._getDashboardData();
	}

	private async _getDashboardData() {
		const { data } = await tryExecute(this, ModelsBuilderService.getModelsBuilderDashboard());
		if (data) {
			this._modelsBuilder = data;
			return true;
		}
		return false;
	}

	private async _onGenerateModels() {
		this._buttonStateBuild = 'waiting';
		const status = await this._postGenerateModels();
		this._buttonStateBuild = status ? 'success' : 'failed';
	}

	private async _postGenerateModels() {
		const { error } = await tryExecute(this, ModelsBuilderService.postModelsBuilderBuild());
		if (error) {
			return false;
		}

		this._getDashboardData();
		return true;
	}

	private async _onDashboardReload() {
		this._buttonStateReload = 'waiting';
		const status = await this._getDashboardData();
		this._buttonStateReload = status ? 'success' : 'failed';
	}

	override render() {
		return html`
			<uui-box headline="Models Builder" class="overview">
				<uui-button
					slot="header-actions"
					.state="${this._buttonStateReload}"
					look="secondary"
					label="Reload"
					@click="${this._onDashboardReload}">
					Reload
				</uui-button>
				<p>Version: ${this._modelsBuilder?.version}</p>
				<div class="models-description">
					<p>ModelsBuilder is enabled with the following configuration:</p>
					<ul>
						${this._modelsBuilder?.mode
							? html`<li>
									The <strong>ModelsMode</strong> is '${this._modelsBuilder.mode}'. ${this.renderModelsMode()}
								</li> `
							: nothing}
						${this.renderList()}
					</ul>
				</div>
				<p class="models-actions">
					${this._modelsBuilder?.outOfDateModels
						? html`<span class="out-of-date">Models are <strong>out-of-date</strong></span>`
						: nothing}
					${this._modelsBuilder?.canGenerate
						? html` <uui-button
								.state="${this._buttonStateBuild}"
								look="primary"
								label="Generate models"
								@click="${this._onGenerateModels}">
								Generate models
							</uui-button>`
						: nothing}
				</p>
				${this._modelsBuilder?.lastError
					? html`<p class="error">Last generation failed with the following error:</p>
							<umb-code-block style="max-height:500px;">${this._modelsBuilder.lastError}</umb-code-block>`
					: nothing}
			</uui-box>
		`;
	}

	private renderList() {
		if (this._modelsBuilder?.mode !== ModelsModeModel.NOTHING) {
			return html`${this._modelsBuilder?.modelsNamespace
				? html`<li>The <strong>models namespace</strong> is ${this._modelsBuilder.modelsNamespace}.</li>`
				: nothing}
			${this._modelsBuilder?.trackingOutOfDateModels === true
				? html`<li>Tracking of <strong>out-of-date models</strong> is enabled.</li>`
				: this._modelsBuilder?.trackingOutOfDateModels === false
					? html`<li>Tracking of <strong>out-of-date models</strong> is not enabled.</li>`
					: nothing}`;
		}
		return nothing;
	}

	renderModelsMode() {
		switch (this._modelsBuilder?.mode) {
			case ModelsModeModel.IN_MEMORY_AUTO:
				return 'Strongly typed models are re-generated on startup and anytime schema changes (i.e. Content Type) are made. No recompilation necessary but the generated models are not available to code outside of Razor.';
			case ModelsModeModel.SOURCE_CODE_MANUAL:
				return 'Strongly typed models are generated on demand. Recompilation is necessary and models are available to all CSharp code.';
			case ModelsModeModel.SOURCE_CODE_AUTO:
				return 'Strong typed models are generated on demand and anytime schema changes (i.e. Content Type) are made. Recompilation is necessary and models are available to all CSharp code.';
			case ModelsModeModel.NOTHING:
				return 'Strongly typed models are not generated. All content and cache will operate from instance of IPublishedContent only.';
			default:
				return;
		}
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-layout-1);
			}

			.models-description ul {
				list-style-type: square;
				margin: 0;
				padding-left: var(--uui-size-layout-1);
			}

			span.out-of-date {
				display: block;
				padding-block-end: var(--uui-size-space-4);
			}

			.error {
				font-weight: bold;
				color: var(--uui-color-danger);
			}

			p.models-actions {
				margin-bottom: 0;
			}
		`,
	];
}

export default UmbModelsBuilderDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-models-builder-dashboard': UmbModelsBuilderDashboardElement;
	}
}
