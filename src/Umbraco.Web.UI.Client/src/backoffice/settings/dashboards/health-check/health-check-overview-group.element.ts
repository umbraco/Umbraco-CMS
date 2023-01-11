import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbHealthCheckContext } from './health-check.context';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { StatusResultType } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';

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
		`,
	];

	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	@state()
	private _checkResponse? = [];

	private _healthCheckContext?: UmbHealthCheckContext;

	render() {
		return html`<a href="${window.location.href + '/' + this.manifest?.meta.label}">
			<uui-box class="group-box"> ${this.manifest?.meta.label} </uui-box>
		</a> `;
	}
}

export default UmbHealthCheckOverviewGroupElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-health-check-overview-group': UmbHealthCheckOverviewGroupElement;
	}
}
