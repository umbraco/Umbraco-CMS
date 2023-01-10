import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import type { ManifestHealthCheck } from '@umbraco-cms/models';
import { StatusResultType } from '@umbraco-cms/backend-api';

@customElement('umb-security-health-check-group2')
export class UmbSecurityHealthCheckGroup2Element extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	manifest?: ManifestHealthCheck;

	@state()
	security = [
		{
			alias: 'applicationUrlConfiguration',
			name: 'Application URL Configuration',
			description: 'Checks if the Umbraco application URL is configured for your site.',
		},
		{
			alias: 'clickJackingProtection',
			name: 'Click-Jacking Protection',
			description:
				'Checks if your site is allowed to be IFRAMEd by another site and thus would be susceptible to click-jacking.',
		},
		{
			alias: 'contentSniffingProtection',
			name: 'Content/MIME Sniffing Protection',
			description: 'Checks that your site contains a header used to protect against MIME sniffing vulnerabilities.',
		},
		{
			alias: 'cookieHijackingProtection',
			name: 'Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))',
			description:
				'Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS).',
		},
		{
			alias: 'crossSiteProtection',
			name: 'Cross-site scripting Protection (X-XSS-Protection header)',
			description:
				'This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.',
		},
		{
			alias: 'excessiveHeaders',
			name: 'Excessive Headers',
			description:
				'Checks to see if your site is revealing information in its headers that gives away unnecessary details about the technology used to build and host it.',
		},
		{
			alias: 'HttpsConfiguration',
			name: 'HTTPS Configuration',
			description:
				'Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.',
		},
	];

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
					${this.security.map((check) => {
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

export default UmbSecurityHealthCheckGroup2Element;
declare global {
	interface HTMLElementTagNameMap {
		'umb-security-health-check-group2': UmbSecurityHealthCheckGroup2Element;
	}
}
