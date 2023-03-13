import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbHealthCheckContext } from '../health-check.context';
import {
	UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN,
	UmbHealthCheckDashboardContext,
} from '../health-check-dashboard.context';
import { ensureSlash, path } from '@umbraco-cms/router';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { HealthCheckGroupWithResultModel, StatusResultTypeModel } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-health-check-group-box-overview')
export class UmbHealthCheckGroupBoxOverviewElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
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

	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	private _healthCheckContext?: UmbHealthCheckDashboardContext;

	private _api?: UmbHealthCheckContext;

	@state()
	private _tagResults?: any = [];

	@state()
	private _keyResults?: HealthCheckGroupWithResultModel;

	constructor() {
		super();

		this.consumeContext(UMB_HEALTHCHECK_DASHBOARD_CONTEXT_TOKEN, (instance) => {
			this._healthCheckContext = instance;
			if (!this._healthCheckContext || !this.manifest?.meta.label) return;
			this._api = this._healthCheckContext?.apis.get(this.manifest?.meta.label);

			this._api?.results.subscribe((results) => {
				this._keyResults = results;
			});
		});
	}

	render() {
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
					<uui-icon name="umb:check"></uui-icon>
					${resultObject.success}
			  </uui-tag> `
			: nothing}
		${resultObject.warning > 0
			? html`<uui-tag look="secondary" color="warning">
					<uui-icon name="umb:alert"></uui-icon>
					${resultObject.warning}
			  </uui-tag>`
			: nothing}
		${resultObject.error > 0
			? html`<uui-tag look="secondary" color="danger">
					<uui-icon name="umb:wrong"></uui-icon>
					${resultObject.error}
			  </uui-tag>`
			: nothing}
		${resultObject.info > 0
			? html`<uui-tag look="secondary">
					<uui-icon name="umb:info"></uui-icon>
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
}

export default UmbHealthCheckGroupBoxOverviewElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-health-check--group-box-overview': UmbHealthCheckGroupBoxOverviewElement;
	}
}
