import type { UmbHealthCheckContext } from '../health-check.context.js';
import type { UmbHealthCheckDashboardContext } from '../health-check-dashboard.context.js';
import { UMB_HEALTHCHECK_DASHBOARD_CONTEXT } from '../health-check-dashboard.context.js';
import type { ManifestHealthCheck } from '../health-check.extension.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, nothing, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { ensureSlash, path } from '@umbraco-cms/backoffice/router';
import type { HealthCheckGroupWithResultResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-health-check-group-box-overview')
export class UmbHealthCheckGroupBoxOverviewElement extends UmbLitElement {
	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	private _healthCheckContext?: UmbHealthCheckDashboardContext;

	private _api?: UmbHealthCheckContext;

	@state()
	private _tagResults?: any = [];

	@state()
	private _keyResults?: HealthCheckGroupWithResultResponseModel;

	constructor() {
		super();

		this.consumeContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT, (instance) => {
			this._healthCheckContext = instance;
			if (!this._healthCheckContext || !this.manifest?.meta.label) return;
			this._api = this._healthCheckContext?.apis.get(this.manifest?.meta.label);

			if (this._api) {
				this.observe(
					this._api.results,
					(results) => {
						this._keyResults = results;
					},
					'_observeApiResults',
				);
			}
		});
	}

	override render() {
		return html`<a href="${ensureSlash(path()) + this.manifest?.meta.label}">
			<uui-box class="group-box"> ${this.manifest?.meta.label} ${this._renderStatus()} </uui-box>
		</a>`;
	}

	_renderStatus() {
		const res: StatusResultTypeModel[] = [];
		this._keyResults?.checks?.forEach((item) => {
			item?.results?.forEach((result) => {
				res.push(result.resultType ?? StatusResultTypeModel.ERROR);
			});
		});
		this._tagResults = res;
		return html`<div>${this._renderCheckResults(this.filterResults(this._tagResults))}</div>`;
	}

	_renderCheckResults(resultObject: any) {
		return html`${resultObject.success > 0
			? html`<uui-tag look="secondary" color="positive">
					<uui-icon name="check"></uui-icon>
					${resultObject.success}
				</uui-tag> `
			: nothing}
		${resultObject.warning > 0
			? html`<uui-tag look="secondary" color="warning">
					<uui-icon name="alert"></uui-icon>
					${resultObject.warning}
				</uui-tag>`
			: nothing}
		${resultObject.error > 0
			? html`<uui-tag look="secondary" color="danger">
					<uui-icon name="remove"></uui-icon>
					${resultObject.error}
				</uui-tag>`
			: nothing}
		${resultObject.info > 0
			? html`<uui-tag look="secondary">
					<uui-icon name="info"></uui-icon>
					${resultObject.info}
				</uui-tag>`
			: nothing} `;
	}

	filterResults(results: any): any {
		const tags = {
			success: 0,
			warning: 0,
			error: 0,
			info: 0,
		};

		results.forEach((result: any) => {
			switch (result) {
				case StatusResultTypeModel.SUCCESS:
					tags.success += 1;
					break;
				case StatusResultTypeModel.WARNING:
					tags.warning += 1;
					break;
				case StatusResultTypeModel.ERROR:
					tags.error += 1;
					break;
				case StatusResultTypeModel.INFO:
					tags.info += 1;
					break;
				default:
					break;
			}
		});
		return tags;
	}

	static override styles = [
		UmbTextStyles,
		css`
			.group-box {
				position: relative;
			}

			.group-box:hover::after {
				content: '';
				width: 100%;
				height: 100%;
				position: absolute;
				top: 0;
				bottom: 0;
				left: 0;
				right: 0;
				border-radius: var(--uui-border-radius);
				transition: opacity 100ms ease-out 0s;
				opacity: 0.33;
				outline-color: var(--uui-color-selected);
				outline-width: 4px;
				outline-style: solid;
			}

			a {
				text-align: center;
				font-weight: bold;
				cursor: pointer;
				text-decoration: none;
				color: var(--uui-color-text);
				margin-bottom: var(--uui-size-space-3);
				display: block;
			}

			uui-icon {
				padding-right: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbHealthCheckGroupBoxOverviewElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-health-check--group-box-overview': UmbHealthCheckGroupBoxOverviewElement;
	}
}
