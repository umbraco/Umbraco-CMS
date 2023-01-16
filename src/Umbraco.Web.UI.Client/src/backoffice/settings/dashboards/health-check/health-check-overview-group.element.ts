import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbHealthCheckContext } from './health-check.context';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { StatusResultType } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbHealthCheckDashboardContext } from './health-check-dashboard.context';
import { result } from 'lodash';
import { ensureSlash, path } from 'router-slot';

@customElement('umb-health-check-overview-group')
export class UmbHealthCheckOverviewGroupElement extends UmbLitElement {
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
				margin-bottom: 10px;
				display: block;
			}

			uui-icon {
				padding-right: var(--uui-size-space-2);
			}
		`,
	];

	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	@state()
	private _checkResponse? = [];

	private _healthCheckContext?: UmbHealthCheckDashboardContext;

	private _api?: UmbHealthCheckContext;

	@state()
	private _results?: any = [];

	@state()
	private _keyResults?: any = [];

	constructor() {
		super();

		this.consumeContext('umbHealthCheckDashboard', (instance) => {
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
		this._keyResults.forEach((item: any) => {
			item.results.forEach((result: any) => {
				this._results.push(result.resultType);
			});
		});
		return html`<div>${this._renderCheckResults(this.filterResults(this._results))}</div>`;
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
				case 'Success':
					tags.success += 1;
					break;
				case 'Warning':
					tags.warning += 1;
					break;
				case 'Error':
					tags.error += 1;
					break;
				case 'Info':
					tags.info += 1;
					break;
				default:
					break;
			}
		});
		return tags;
	}
}

export default UmbHealthCheckOverviewGroupElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-health-check-overview-group': UmbHealthCheckOverviewGroupElement;
	}
}
