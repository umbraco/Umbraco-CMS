import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { StatusResultType } from '@umbraco-cms/backend-api';

@customElement('umb-health-check-group-two')
export class UmbSecurityHealthCheckGroupTwoElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	@state()
	private _checkResponse? = [];

	private async checkGroup() {
		console.log('group checked');

		// Default options are marked with *
		const url = '/umbraco/management/api/v1/health-check/Security/';
		const response = await fetch(url, {
			method: 'GET', // *GET, POST, PUT, DELETE, etc.
			mode: 'cors', // no-cors, *cors, same-origin
			cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
			credentials: 'same-origin', // include, *same-origin, omit
			headers: {
				'Content-Type': 'application/json',
				// 'Content-Type': 'application/x-www-form-urlencoded',
			},
		});

		this._checkResponse = await response.json(); // parses JSON response into native JavaScript objects
		console.log(this._checkResponse);
	}

	render() {
		return html`
			<uui-box>
				<div slot="headline" class="flex">
					<span>${this.manifest?.meta.label}</span>
					<uui-button color="positive" look="primary" @click="${this.checkGroup}"> Check group </uui-button>
				</div>
				<div class="checks-wrapper">
					${this.manifest?.meta.checks.map((check) => {
						return html`<uui-box headline="${check.name || '?'}">
							<p>${check.description}</p>
							${this.renderCheckResults(check.alias)}
						</uui-box>`;
					})}
				</div>
			</uui-box>
		`;
	}

	renderCheckResults(alias: string) {
		const checkResults = this._checkResponse?.find((result: any) => result.alias === alias);
		return html`<uui-icon-registry-essential>
			<div class="data">
				${checkResults?.results.map((result: any) => {
					return html`<div class="result-wrapper">
						<p>${this.renderIcon(result.resultType)} ${result.message}</p>
						${result.readMoreLink ? html`<uui-button color="default" look="primary">Read more</uui-button>` : nothing}
					</div>`;
				})}
			</div>
		</uui-icon-registry-essential>`;
	}

	private renderIcon(type?: StatusResultType) {
		switch (type) {
			case 'Success':
				return html`<uui-icon style="color: var(--uui-color-positive);" name="check"></uui-icon>`;
			case 'Warning':
				return html`<uui-icon style="color: var(--uui-color-warning);" name="alert"></uui-icon>`;
			case 'Error':
				return html`<uui-icon style="color: var(--uui-color-danger);" name="remove"></uui-icon>`;
			case 'Info':
				return html`<uui-icon style="color:black;" name="info"></uui-icon>`;
			default:
				return nothing;
		}
	}
}

export default UmbSecurityHealthCheckGroupTwoElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-health-check-group-two': UmbSecurityHealthCheckGroupTwoElement;
	}
}
