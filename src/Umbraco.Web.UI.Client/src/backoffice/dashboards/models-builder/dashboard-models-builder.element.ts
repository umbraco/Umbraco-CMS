import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { state, customElement } from 'lit/decorators.js';
import { unsafeHTML } from 'lit/directives/unsafe-html.js';

import { UUIButtonState } from '@umbraco-ui/uui';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbNotificationService, UmbNotificationDefaultData } from '@umbraco-cms/services';
import {
	ApiError,
	CreatedResult,
	ModelsBuilder,
	ModelsBuilderResource,
	ProblemDetails,
} from '@umbraco-cms/backend-api';

import 'src/core/utils/errorbox';

@customElement('umb-dashboard-models-builder')
export class UmbDashboardModelsBuilderElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			.headline {
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			p {
				margin-block-start: 0;
				margin-block-end: var(--uui-size-space-4);
			}

			.models-description p {
				padding-bottom: var(--uui-size-space-1);
				margin-bottom: var(--uui-size-space-1);
			}

			.models-description ul {
				list-style-type: square;
				margin: 0;
				padding-left: var(--uui-size-layout-1);
			}

			.error {
				padding-top: var(--uui-size-space-5);
				font-weight: bold;
				color: var(--uui-color-danger);
			}
		`,
	];

	private _notificationService?: UmbNotificationService;

	@state()
	private _modelsBuilder?: ModelsBuilder;

	@state()
	private _createdResult?: CreatedResult;

	@state()
	private _buttonStateBuild: UUIButtonState = undefined;

	@state()
	private _buttonStateReload: UUIButtonState = undefined;

	private async _getDashboardData() {
		try {
			const modelsBuilder = await ModelsBuilderResource.getModelsBuilderDashboard();
			this._modelsBuilder = modelsBuilder;
			return true;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = {
					message: error.message ?? 'Something went wrong',
				};
				this._notificationService?.peek('danger', { data });
			}
			return false;
		}
	}

	private async _onGenerateModels() {
		this._buttonStateBuild = 'waiting';
		const status = await this._postGenerateModels();
		this._buttonStateBuild = status ? 'success' : 'failed';
	}

	private async _postGenerateModels() {
		try {
			const createdResult = await ModelsBuilderResource.postModelsBuilderBuild();
			this._createdResult = createdResult;
			return true;
		} catch (e) {
			if (e instanceof ApiError) {
				const error = e as ProblemDetails;
				const data: UmbNotificationDefaultData = {
					message: error.message ?? 'Model generation failed',
				};
				this._notificationService?.peek('danger', { data });
			}
			return false;
		}
	}

	constructor() {
		super();
		this._getDashboardData();
		this.consumeAllContexts(['umbNotificationService'], (instances) => {
			this._notificationService = instances['umbNotificationService'];
		});
	}

	private async _onDashboardReload() {
		this._buttonStateReload = 'waiting';
		const status = await this._getDashboardData();
		this._buttonStateReload = status ? 'success' : 'failed';
	}

	render() {
		return html`
			<uui-box>
				<div class="headline" slot="headline">
					<strong>Models Builder</strong>
					<uui-button .state="${this._buttonStateReload}" look="secondary" @click="${this._onDashboardReload}">
						Reload
					</uui-button>
				</div>
				<p>Version: ${this._modelsBuilder?.version}</p>

				<div class="models-description">
					<p>${unsafeHTML(this._modelsBuilder?.modelsNamespace)}</p>
				</div>

				${this._modelsBuilder?.outOfDateModels === true
					? html`<p>Models are <strong>out of date</strong></p>`
					: nothing}
				${this._modelsBuilder?.canGenerate === true
					? html` <uui-button
							.state="${this._buttonStateBuild}"
							look="primary"
							label="Generate models"
							@click="${this._onGenerateModels}">
							Generate models
					  </uui-button>`
					: nothing}
				${this._modelsBuilder?.lastError
					? html`<p class="error">Last generation failed with the following error:</p>
							<uui-error-box>${this._modelsBuilder.lastError}</uui-error-box>`
					: nothing}
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-models-builder': UmbDashboardModelsBuilderElement;
	}
}
